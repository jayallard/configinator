using System.Collections.Generic;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class PropertyValue
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public List<PropertyHistoryItem> History { get; } = new();
    }
}