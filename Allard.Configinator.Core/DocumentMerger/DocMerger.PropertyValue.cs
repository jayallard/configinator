using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.DocumentMerger
{
    [DebuggerDisplay("{Name} = {Value}")]
    public class PropertyValue
    {
        public string Name { get; set; }
        public object Value => Layers.Last()?.Value;
        public List<PropertyLayer> Layers { get; } = new();
    }
}