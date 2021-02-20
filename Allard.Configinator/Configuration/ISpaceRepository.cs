using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
{
    public interface ISpaceRepository
    {
        Task<IEnumerable<Space>> GetSpaces();
        Task<Space> GetSpace(string name);
    }
}