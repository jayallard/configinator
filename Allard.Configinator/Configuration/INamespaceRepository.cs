using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
{
    public interface INamespaceRepository
    {
      public Task<IEnumerable<NamespaceDto>> GetNamespaces(); 
    } 
}