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
    public class ConfiginatorService
    {
        private readonly SchemaParser parser;

        /// <summary>
        /// Work with the storage of configuration values.
        /// </summary>
        private readonly IConfigStore configStore;

        /// <summary>
        /// Works with the storage of space configuration info.
        /// </summary>
        private readonly ISpaceRepository spaceRepository;

        private readonly ISchemaMetaRepository schemaMetaRepository;
        private readonly INamespaceRepository namespaceRepository;
        private ConcurrentDictionary<string, Space> spaces;
        private ConcurrentDictionary<string, ConfigurationNamespace> namespaces;

        public ConfiginatorService(
            SchemaParser parser,
            IConfigStore configStore,
            ISpaceRepository spaceRepository,
            INamespaceRepository namespaceRepository)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            this.spaceRepository =
                spaceRepository ?? throw new ArgumentNullException(nameof(spaceRepository));
            this.namespaceRepository = namespaceRepository ??
                                           throw new ArgumentNullException(nameof(namespaceRepository));
        }

        private async Task LoadSpaces()
        {
            if (spaces != null)
            {
                return;
            }

            var spaceMap = (await spaceRepository.GetSpaces())
                .ToDictionary(s => s.Name);
            spaces = new ConcurrentDictionary<string, Space>(spaceMap);
        }

        public async Task<IEnumerable<Space>> GetSpaces()
        {
            await LoadSpaces();
            return spaces.Values;
        }

        public async Task<Space> GetSpace(string spaceName)
        {
            await LoadSpaces();
            return spaces[spaceName];
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

        public async Task Save(ConfigurationSectionValue value)
        {
        }
    }
}