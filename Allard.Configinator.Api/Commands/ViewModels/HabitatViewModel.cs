using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record HabitatViewModel(string HabitatName, IReadOnlyCollection<string> bases);
}