using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.DocumentMerger
{
    [DebuggerDisplay("Path={Path}, PropertyName={Name}, Value={Value}")]
    public record PropertyValue(string Path, string Name, List<PropertyLayer> Layers)
    {
        public string Value => Layers.LastOrDefault()?.Value;
    }

    public record ObjectValue(string Path, string Name, IReadOnlyCollection<PropertyValue> Properties,
        IReadOnlyCollection<ObjectValue> Objects);
}