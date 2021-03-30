using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.DocumentMerger
{
    [DebuggerDisplay("ObjectPath={ObjectPath}, PropertyName={Name}, Value={Value}")]
    public record PropertyValue(string ObjectPath, string Name, List<PropertyLayer> Layers)
    {
        public string Value => Layers.LastOrDefault()?.Value;

        public string GetValue(string layerName)
        {
            return Layers.Single(l => l.LayerName == layerName).Value;
        }
    }

    public record ObjectValue(string ObjectPath, string Name, IReadOnlyCollection<PropertyValue> Properties,
        IReadOnlyCollection<ObjectValue> Objects);
}