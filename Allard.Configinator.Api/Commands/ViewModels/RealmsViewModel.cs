using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record RealmsViewModel
    {
        public IEnumerable<RealmViewModel> Realms { get; init; }
        public IEnumerable<Link> Links { get; init; }
    }
}