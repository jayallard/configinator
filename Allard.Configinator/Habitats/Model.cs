using System.Collections.Generic;

namespace Allard.Configinator.Habitats
{
    public record Habitat(string Name, string Description, IReadOnlySet<string> Bases);
}