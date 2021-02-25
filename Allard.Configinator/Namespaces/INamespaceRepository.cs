using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Namespaces
{
    public interface INamespaceRepository
    {
        public Task<IEnumerable<NamespaceDto>> GetNamespaces();
    }
}