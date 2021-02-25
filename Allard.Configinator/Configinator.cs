using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
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
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            this.habitatRepository =
                habitatRepository ?? throw new ArgumentNullException(nameof(habitatRepository));
            this.namespaceRepository = namespaceRepository ??
                                       throw new ArgumentNullException(nameof(namespaceRepository));
        }

        private async Task LoadHabitats()
        {
            if (habitats != null) return;
            var spaceMap = (await habitatRepository.GetHabitats())
                .ToDictionary(s => s.Name);
            habitats = new ConcurrentDictionary<string, Habitat>(spaceMap);
        }

        public async Task<IEnumerable<Habitat>> GetHabitats()
        {
            if (habitats == null)
            {
                await LoadHabitats();
            }

            return habitats.Values;
        }

        public async Task<Habitat> GetHabitatAsync(string habitatName)
        {
            habitatName = string.IsNullOrWhiteSpace(habitatName)
                ? throw new ArgumentNullException(nameof(habitatName))
                : habitatName;
            await LoadHabitats();
            return habitats[habitatName];
        }

        private async Task LoadNamespaces()
        {
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
            ns = ns ?? throw new ArgumentNullException(nameof(ns));
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

        public async Task<IEnumerable<ConfigurationNamespace>> GetNamespacesAsync()
        {
            await LoadNamespaces();
            return namespaces.Values;
        }

        public async Task<ConfigurationNamespace> GetNamespaceAsync(string nameSpace)
        {
            nameSpace = string.IsNullOrWhiteSpace(nameSpace)
                ? throw new ArgumentNullException(nameof(nameSpace))
                : nameSpace;
            await LoadNamespaces();
            return namespaces[nameSpace];
        }

        public async Task SetValueAsync(ConfigurationSectionValue value)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            // TODO: validate habitat and config section are valid.
            await configStore.SetValueAsync(value);
        }

        public async Task<ConfigurationSectionValue> GetValueAsync(
            string habitat,
            string nameSpace,
            string configSection)
        {
            habitat = string.IsNullOrWhiteSpace(habitat) ? throw new ArgumentNullException(nameof(habitat)) : habitat;
            nameSpace = string.IsNullOrWhiteSpace(nameSpace)
                ? throw new ArgumentNullException(nameof(nameSpace))
                : nameSpace;
            configSection = string.IsNullOrWhiteSpace(configSection)
                ? throw new ArgumentNullException(nameof(configSection))
                : configSection;

            // todo: prevent circular references
            var h = await GetHabitatAsync(habitat);
            var ns = await GetNamespaceAsync(nameSpace);
            var cs = ns.ConfigurationSections.Single(c => c.Id.Name == configSection);

            // get the base values
            var baseValues = h
                .Bases
                .Select(async hab => await GetValueAsync(hab, nameSpace, configSection))
                .ToList();

            // get the requested value
            var value = configStore.GetValue(h, cs);
            await Task.WhenAll(baseValues).ConfigureAwait(false);
            await value;

            // todo: base can't have null values. no point inheriting if it doesn't exist.
            var all = baseValues
                .Where(b => b.Result?.Value != null)
                .Select(b => b.Result.Value).ToList();
            if (value.Result?.Value != null) all.Add(value.Result.Value);

            all.Reverse();

            var docs = all.Select(JToken.Parse).ToList();
            var final = new JsonMerger(docs).Merge()?.ToString();

            // TODO: indicate that the value doesn't exist.
            return new ConfigurationSectionValue(h, cs, string.Empty, final);
        }
    }
}