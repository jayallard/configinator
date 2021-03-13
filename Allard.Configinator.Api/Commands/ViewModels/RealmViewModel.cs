using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class RealmViewModel
    {
        // TODO: habitats

        public string RealmId { get; set; }
        public List<ConfigurationSectionViewModel> ConfigurationSections { get; set; }
        public List<Link> Links { get; set; }
        public List<HabitatViewModel> Habitats { get; set; }
    }
}