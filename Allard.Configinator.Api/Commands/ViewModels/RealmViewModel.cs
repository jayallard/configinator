using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class RealmViewModel
    {
        // TODO: habitats
        
        public string RealmName { get; set; }
        public List<ConfigurationSectionViewModel> ConfigurationSections { get; set; }
        public List<Link> Links { get; set; }
        public List<HabitatViewModel> Habitats { get; set; }
        
    }
}