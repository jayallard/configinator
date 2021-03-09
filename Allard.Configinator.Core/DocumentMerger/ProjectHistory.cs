using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class PropertyHistoryItem
    {
        public OrderedDocumentToMerge DocName { get; set; }
        public IObjectNode Object { get; set; }
        public IPropertyNode Property { get; set; }
        public Transition Transition { get; set; }
        public string ActionTypeString => Transition.ToString();
        public string ReferencedDoc { get; set; }
        public object Value { get; set; }
    }
}