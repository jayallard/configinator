using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

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

    public static class ExtensionMethods2
    {
        public static IEnumerable<JsonProperty> GetObjects2(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.Object);
        }

        public static IEnumerable<JsonProperty> GetProperties2(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.String);
        }

        public static JsonElement GetStringProperty2(this JsonElement parentElement, string propertyName)
        {
            if (parentElement.ValueKind == JsonValueKind.Undefined) return parentElement;
            if (parentElement.TryGetProperty(propertyName, out var existing))
            {
                return existing.ValueKind == JsonValueKind.String
                    ? existing
                    : default;
            }

            return default;
        }

        public static JsonElement GetObjectProperty2(this JsonElement parentElement, string propertyName)
        {
            if (parentElement.ValueKind == JsonValueKind.Undefined) return parentElement;
            if (parentElement.TryGetProperty(propertyName, out var existing))
            {
                return existing.ValueKind == JsonValueKind.Object
                    ? existing
                    : default;
            }

            return default;
        }
    }
}