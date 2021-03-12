using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record HabitatViewModel(string HabitatId, IReadOnlyCollection<string> BaseHabitatIds);
}