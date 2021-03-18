using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public record HabitatViewModel(string HabitatId, IReadOnlyCollection<string> BaseHabitatIds);
}