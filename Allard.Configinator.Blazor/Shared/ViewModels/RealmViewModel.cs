using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Api;
using Allard.Configinator.Api.Commands.ViewModels;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public class RealmViewModel
    {
        // TODO: habitats

        public string RealmId { get; set; }
        public List<ConfigurationSectionViewModel> ConfigurationSections { get; set; }
        public List<Link> Links { get; set; }
        public List<HabitatViewModel> Habitats { get; set; }

        public HabitatViewModel GetHabitat(string habitatId)
        {
            return Habitats.Single(h => h.HabitatId == habitatId);
        }
    }
}