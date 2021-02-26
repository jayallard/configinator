using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Namespaces
{
    public class NamespaceService : INamespaceService
    {
        private readonly INamespaceRepository repository;
        private readonly ISchemaService schemaService;
        private Dictionary<string, ConfigurationNamespace> namespaces;

        public NamespaceService(INamespaceRepository repository, ISchemaService schemaService)
        {
            this.repository = repository.EnsureValue(nameof(repository));
            this.schemaService = schemaService.EnsureValue(nameof(schemaService));
        }

        public async Task<ConfigurationNamespace> GetNamespaceAsync(string name)
        {
            if (namespaces == null) await Load().ConfigureAwait(false);
            Debug.Assert(namespaces != null);
            if (namespaces.TryGetValue(name, out var ns)) return ns;
            throw new NamespaceNotFoundException(name);
        }

        public async Task<IEnumerable<ConfigurationNamespace>> GetNamespacesAsync()
        {
            if (namespaces == null) await Load().ConfigureAwait(false);
            Debug.Assert(namespaces != null);
            return namespaces.Values;
        }

        private async Task<ConfigurationNamespace> ToNamespace(NamespaceDto ns)
        {
            var sectionTasks = ns
                .Sections
                .Select(async s =>
                {
                    var sectionId = new ConfigurationSectionId(ns.Name, s.Name);
                    var type = await schemaService.GetSchemaTypeAsync(s.Type).ConfigureAwait(false);
                    return new ConfigurationSection(sectionId, s.Path, type, s.Description);
                })
                .ToList();

            await Task.WhenAll(sectionTasks).ConfigureAwait(false);
            var sections = sectionTasks
                .Select(t => t.Result)
                .ToList()
                .AsReadOnly();
            return new ConfigurationNamespace(ns.Name, sections);
        }

        private async Task Load()
        {
            var tasks = (await repository.GetNamespaces().ConfigureAwait(false))
                .Select(async n => await ToNamespace(n).ConfigureAwait(false))
                .ToList();
            var dto = await Task.WhenAll(tasks).ConfigureAwait(false);
            namespaces = dto.ToDictionary(d => d.Name);
        }
    }
}