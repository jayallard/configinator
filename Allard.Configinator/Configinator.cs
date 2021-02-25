using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Habitats;
using Allard.Configinator.Namespaces;
using Allard.Configinator.Schema;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator
{
    public class Configinator
    {
        /// <summary>
        ///     Work with the storage of configuration values.
        /// </summary>
        private readonly IConfigStore configStore;

        /// <summary>
        ///     Habit configuration store.
        /// </summary>
        private readonly IHabitatRepository habitatRepository;

        /// <summary>
        ///     Namespace configuration store.
        /// </summary>
        private readonly INamespaceRepository namespaceRepository;

        private readonly ISchemaService service;
        private ConcurrentDictionary<string, Habitat> habitats;
        private ConcurrentDictionary<string, ConfigurationNamespace> namespaces;

        public Configinator(
            ISchemaService service,
            IConfigStore configStore,
            IHabitatRepository habitatRepository,
            INamespaceRepository namespaceRepository)
        {
            this.service = service.EnsureValue(nameof(service));
            this.configStore = configStore.EnsureValue(nameof(configStore));
            this.habitatRepository = habitatRepository.EnsureValue(nameof(habitatRepository));
            this.namespaceRepository = namespaceRepository.EnsureValue(nameof(namespaceRepository));
        }

        private async Task LoadHabitats()
        {
            if (habitats != null) return;
            var values = (await habitatRepository.GetHabitats())
                .ToDictionary(s => s.Name);
            habitats = new ConcurrentDictionary<string, Habitat>(values);
        }

        private async Task<Habitat> GetHabitatAsync(string habitatName)
        {
            // todo: move to habitat service
            habitatName.EnsureValue(nameof(habitatName));
            await LoadHabitats();
            return habitats[habitatName];
        }

        private async Task LoadNamespaces()
        {
            // todo: move to namespace service
            if (namespaces != null) return;
            var nsDtos = await namespaceRepository.GetNamespaces();
            var nsTasks = nsDtos
                .Select(async n => await ToNamespace(n))
                .ToList();
            await Task.WhenAll(nsTasks);
            var values = nsTasks
                .Select(t => t.Result)
                .ToDictionary(d => d.Name);
            namespaces = new ConcurrentDictionary<string, ConfigurationNamespace>(values);
        }

        private async Task<ConfigurationNamespace> ToNamespace(NamespaceDto ns)
        {
            var sectionTasks = ns
                .Sections
                .Select(async s =>
                {
                    var sectionId = new ConfigurationSectionId(ns.Name, s.Name);
                    var type = await service.GetSchemaTypeAsync(s.Type);
                    return new ConfigurationSection(sectionId, s.Path, type, s.Description);
                })
                .ToList();

            await Task.WhenAll(sectionTasks);
            var sections = sectionTasks
                .Select(t => t.Result)
                .ToList()
                .AsReadOnly();
            return new ConfigurationNamespace(ns.Name, sections);
        }

        private async Task<ConfigurationNamespace> GetNamespaceAsync(string nameSpace)
        {
            await LoadNamespaces();
            return namespaces[nameSpace];
        }

        private async Task<string> GetPath(ConfigurationId id)
        {
            var ns = await GetNamespaceAsync(id.Namespace);
            var cs = ns.GetConfigurationSection(id.ConfigurationSection);
            var habitat = await GetHabitatAsync(id.Habitat);
            return cs.Path.Replace("{{habitat}}", habitat.Name);
        }

        public async Task SetValueAsync(ConfigurationSectionValue value)
        {
            value.EnsureValue(nameof(value));
            var path = await GetPath(value.Id);
            await configStore.SetValueAsync(new ConfigurationValue(path, value.Etag, value.Value));
        }

        public async Task<ConfigurationSectionValue> GetValueAsync(ConfigurationId id)
        {
            id.EnsureValue(nameof(id));

            var habitat = await GetHabitatAsync(id.Habitat);

            // --------------------------------------------------
            // get the values from the base paths.
            // --------------------------------------------------
            var baseValues = habitat
                .Bases
                .Select(async baseHabitat =>
                {
                    var baseId = new ConfigurationId(baseHabitat, id.Namespace, id.ConfigurationSection);
                    return await GetValueAsync(baseId);
                })
                .ToList();

            // --------------------------------------------------
            // get the value from the requested path,
            // then wait for the base queries to finish.
            // --------------------------------------------------
            var value = await configStore.GetValueAsync(await GetPath(id));
            await Task.WhenAll(baseValues).ConfigureAwait(false);

            // --------------------------------------------------
            // put them all together, then merge.
            // --------------------------------------------------
            var all = baseValues
                .Where(b => b.Result?.Value != null)
                .Select(b => b.Result.Value)
                .ToList();
            if (value.Value != null)
            {
                all.Add(value.Value);
            }

            all.Reverse();

            var docs = all.Select(JToken.Parse).ToList();
            var final = new JsonMerger(docs).Merge()?.ToString();

            // TODO: etag only represents the bottom most layer - misleading. if top level changes,
            // then value is different, but etag is the same.
            // TODO: indicate that the value doesn't exist.
            return new ConfigurationSectionValue(id, value.ETag, final);
        }
    }
}