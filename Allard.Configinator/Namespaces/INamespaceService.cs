using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Namespaces
{
    public interface INamespaceService
    {
        Task<ConfigurationNamespace> GetNamespaceAsync(string name);
        Task<IEnumerable<ConfigurationNamespace>> GetNamespacesAsync();
    }
}