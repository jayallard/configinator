using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Habitats;

namespace Allard.Configinator
{
    public class HabitatsAccessor
    {
        private readonly IHabitatService habitatService;

        public HabitatsAccessor(IHabitatService service)
        {
            habitatService = service.EnsureValue(nameof(service));
        }

        public async Task<Habitat> ByName(string habitatName)
        {
            return await habitatService.GetHabitatAsync(habitatName).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Habitat>> All()
        {
            return await habitatService.GetHabitatsAsync().ConfigureAwait(false);
        }
    }
}