using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Realms
{
    public class RealmService : IRealmService
    {
        private readonly IRealmRepository repository;
        private readonly ISchemaService schemaService;
        private Dictionary<string, Realm> realms;

        public RealmService(IRealmRepository repository, ISchemaService schemaService)
        {
            this.repository = repository.EnsureValue(nameof(repository));
            this.schemaService = schemaService.EnsureValue(nameof(schemaService));
        }

        public async Task<Realm> GetRealmAsync(string name)
        {
            if (realms == null) await Load().ConfigureAwait(false);
            Debug.Assert(realms != null);
            if (realms.TryGetValue(name, out var ns)) return ns;
            throw new RealmNotFoundException(name);
        }

        public async Task<IEnumerable<Realm>> GetRealmsAsync()
        {
            if (realms == null) await Load().ConfigureAwait(false);
            Debug.Assert(realms != null);
            return realms.Values;
        }

        private async Task<Realm> ToRealm(RealmStorageDto ns)
        {
            var sectionTasks = ns
                .ConfigurationSections
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
            return new Realm(ns.Name, sections);
        }

        private async Task Load()
        {
            var tasks = (await repository.GetRealms().ConfigureAwait(false))
                .Select(async n => await ToRealm(n).ConfigureAwait(false))
                .ToList();
            var dto = await Task.WhenAll(tasks).ConfigureAwait(false);
            realms = dto.ToDictionary(d => d.Name);
        }
    }
}