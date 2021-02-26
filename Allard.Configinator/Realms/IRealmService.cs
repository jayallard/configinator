using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Realms
{
    public interface IRealmService
    {
        Task<Realm> GetRealmAsync(string name);
        Task<IEnumerable<Realm>> GetRealmsAsync();
    }
}