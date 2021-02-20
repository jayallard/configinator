using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;

namespace Allard.Configinator
{
    public class Configinator
    {
        private readonly SchemaParser parser;

        /// <summary>
        /// Work with the storage of configuration values.
        /// </summary>
        private readonly IConfigStore configStore;

        /// <summary>
        /// Works with the storage of space configuration info.
        /// </summary>
        private readonly IHabitatRepository habitatRepository;

        private readonly ISchemaMetaRepository schemaMetaRepository;
        private readonly INamespaceRepository namespaceRepository;
        private ConcurrentDictionary<string, Habitat> habitats;
        private ConcurrentDictionary<string, ConfigurationNamespace> namespaces;

        public Configinator(
            SchemaParser parser,
            IConfigStore configStore,
            IHabitatRepository habitatRepository,
            INamespaceRepository namespaceRepository)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            this.habitatRepository =
                habitatRepository ?? throw new ArgumentNullException(nameof(habitatRepository));
            this.namespaceRepository = namespaceRepository ??
                                           throw new ArgumentNullException(nameof(namespaceRepository));
        }

        private async Task LoadHabitats()
        {
            if (habitats != null)
            {
                return;
            }

            var spaceMap = (await habitatRepository.GetHabitats())
                .ToDictionary(s => s.Name);
            habitats = new ConcurrentDictionary<string, Habitat>(spaceMap);
        }

        public async Task<IEnumerable<Habitat>> GetHabitats()
        {
            await LoadHabitats();
            return habitats.Values;
        }

        public async Task<Habitat> GetHabitat(string habitatName)
        {
            await LoadHabitats();
            return habitats[habitatName];
        }

        private async Task LoadNamespaces()
        {
            if (namespaces != null)
            {
                return;
            }

            var x = await namespaceRepository.GetNamespaces();
            var nsTasks = x
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
                    var type = await parser.GetSchemaType(s.Type);
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


        public async Task<IEnumerable<ConfigurationNamespace>> GetNamespaces()
        {
            await LoadNamespaces();
            return namespaces.Values;
        }

        public async Task<ConfigurationNamespace> GetNamespace(string nameSpace)
        {
            await LoadNamespaces();
            return namespaces[nameSpace];
        }

        public async Task SetValueAsync(ConfigurationSectionValue value)
        {
            // TODO: validate habitat and config section are valid.
            
            await configStore.SetValueAsync(value);
        }

        public async Task<ConfigurationSectionValue> GetValueAsync(string habitat, string nameSpace, string configSection)
        {
            var h = await GetHabitat(habitat);
            var ns = await GetNamespace(nameSpace);
            var cs = ns.ConfigurationSections.Single(c => c.Id.Name == configSection);
            var value = await this.configStore.GetValue(h, cs);
            
            // TODO: indicate that the value doesn't exist.
            return value ?? new ConfigurationSectionValue(h, cs, null, null);
        }
    }
}