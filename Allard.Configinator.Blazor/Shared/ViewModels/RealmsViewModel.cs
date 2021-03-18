using System.Collections.Generic;
using Allard.Configinator.Api;
using Allard.Configinator.Api.Commands.ViewModels;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public record RealmsViewModel
    {
        public List<RealmViewModel> Realms { get; init; }
        public List<Link> Links { get; set; }
    }
}