using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record DocumentToMerge(string Name, JsonDocument Document);

    public record OrderedDocumentToMerge(DocumentToMerge Doc, int Order);
}