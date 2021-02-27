using System.Collections.Generic;
using Allard.Configinator.Api.Commands.ViewModels;

namespace Allard.Configinator.Api.Commands
{
    public class ConfigurationSectionViewModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string TypeId { get; set; }
        public string Realm { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}