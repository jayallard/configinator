using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Realms
{
    public interface IRealmRepository
    {
        public Task<IEnumerable<RealmStorageDto>> GetRealms();
    }
}