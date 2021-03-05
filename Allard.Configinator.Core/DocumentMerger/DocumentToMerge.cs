using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record DocumentToMerge(string Name, int Order, IObjectNode Doc);
}