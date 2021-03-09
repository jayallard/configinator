using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record DocumentToMerge(string Name, IObjectNode Document);

    public record OrderedDocumentToMerge(DocumentToMerge Doc, int Order);
}