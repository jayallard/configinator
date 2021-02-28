using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class RealmViewModel
    {
        public string Name { get; set; }
        public IEnumerable<ConfigurationSectionViewModel> ConfigurationSections { get; set; }
        public IEnumerable<Link> Links { get; set; }

        public RealmViewModel SetLinks(IEnumerable<Link> links)
        {
            Links = links?.ToList();
            return this;
        }
    }
}