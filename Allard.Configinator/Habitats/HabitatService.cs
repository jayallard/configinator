using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Allard.Configinator.Habitats
{
    public class HabitatService : IHabitatService
    {
        private readonly IHabitatRepository repo;
        private Dictionary<string, Habitat> habitats;

        public HabitatService(IHabitatRepository repo)
        {
            this.repo = repo.EnsureValue(nameof(repo));
        }

        public async Task<Habitat> GetHabitatAsync(string name)
        {
            if (habitats == null) await Load().ConfigureAwait(false);
            Debug.Assert(habitats != null);
            if (habitats.TryGetValue(name, out var habitat)) return habitat;

            throw new HabitatNotFoundException(name);
        }

        public async Task<IEnumerable<Habitat>> GetHabitatsAsync()
        {
            if (habitats == null) await Load().ConfigureAwait(false);
            Debug.Assert(habitats != null);
            return habitats.Values;
        }

        private async Task Load()
        {
            var habitatData = await repo.GetHabitats().ConfigureAwait(false);
            habitats = habitatData.ToDictionary(h => h.Name);
        }
    }
}