using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Habitats
{
    public interface IHabitatService
    {
        Task<Habitat> GetHabitatAsync(string name);
        Task<IEnumerable<Habitat>> GetHabitatsAsync();
    }
}