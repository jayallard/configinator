using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class RealmViewModel
    {
        public string Name { get; set; }
        public List<ConfigurationSectionViewModel> ConfigurationSections { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}