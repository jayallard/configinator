using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public record HabitatViewModel(string HabitatId, IReadOnlyCollection<string> BaseHabitatIds);
}