using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record RealmsViewModel
    {
        public List<RealmViewModel> Realms { get; init; }
        public List<Link> Links { get; set; }
    }
}