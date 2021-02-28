using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class RealmsViewModel
    {
        public IEnumerable<RealmViewModel> Realms { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}