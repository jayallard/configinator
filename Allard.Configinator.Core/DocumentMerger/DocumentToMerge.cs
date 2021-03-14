using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record DocumentToMerge(string Name, JsonDocument Document);
}