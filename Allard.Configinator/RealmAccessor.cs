using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Realms;

namespace Allard.Configinator
{
    public class RealmAccessor
    {
        private readonly IRealmService service;

        public RealmAccessor(IRealmService service)
        {
            this.service = service.EnsureValue(nameof(service));
        }

        public async Task<Realm> ByName(string name)
        {
            return await service.GetRealmAsync(name).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Realm>> All()
        {
            return await service.GetRealmsAsync().ConfigureAwait(false);
        }
    }
}