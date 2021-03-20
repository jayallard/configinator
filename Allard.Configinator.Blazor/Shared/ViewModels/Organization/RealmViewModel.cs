using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class RealmViewModel
    {
        public string RealmId { get; set; }
        public List<ConfigurationSectionViewModel> ConfigurationSections { get; set; } = new();
        public List<Link> Links { get; set; }
        public List<HabitatViewModel> Habitats { get; set; } = new();

        public HabitatViewModel GetHabitat(string habitatId)
        {
            return Habitats.Single(h => h.HabitatId == habitatId);
        }
    }
}