using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Habitats
{
    public interface IHabitatRepository
    {
        Task<IEnumerable<Habitat>> GetHabitats();
    }
}