using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
{
    public interface IHabitatRepository
    {
        Task<IEnumerable<Habitat>> GetHabitats();
        Task<Habitat> GetSpace(string name);
    }
}