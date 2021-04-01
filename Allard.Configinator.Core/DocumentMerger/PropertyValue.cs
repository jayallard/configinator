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

    public class JsonVersionedPropertyValue
    {
        private readonly JsonElement valueElement;
        public bool Exists => valueElement.ValueKind != JsonValueKind.Undefined;

        public string OriginalValue { get; }
        public string Value { get; private set; }
        public string VersionName { get; }
        public bool IsChanged => !string.Equals(Value, OriginalValue);

        public bool IsSet { get; private set; }

        public void SetValue(string value)
        {
            IsSet = true;
            Value = value;
        }

        public JsonVersionedPropertyValue(string versionName, JsonElement valueElement)
        {
            VersionName = versionName.EnsureValue(nameof(versionName));
            this.valueElement = valueElement;
            if (valueElement.ValueKind == JsonValueKind.String)
            {
                OriginalValue = Value = valueElement.GetString();
            }
        }
    }

    public class JsonVersionedProperty
    {
        private readonly Dictionary<string, JsonVersionedPropertyValue> values = new();
        private readonly JsonElement model;
        public string ObjectPath { get; }

        public JsonVersionedProperty(string objectPath, JsonElement model)
        {
            ObjectPath = objectPath.EnsureValue(nameof(objectPath));
            this.model = model;
        }

        public void AddVersion(string version, JsonElement value)
        {
            // TODO: make sure version doesn't exist
            version.EnsureValue(nameof(version));
            var v = new JsonVersionedPropertyValue(version, value);
            values[version] = v;
        }

        public bool IsChanged => values.Values.Any(v => v.IsChanged);
    }

    public class JsonVersionedObject
    {
        private readonly string path;
        private readonly JsonElement model;
        private readonly Dictionary<string, JsonVersionedProperty> properties = new();
        private readonly Dictionary<string, JsonVersionedObject> objects = new();
        private readonly List<JsonProperty> modelProperties;
        private readonly List<JsonProperty> modelObjects;

        private JsonVersionedObject(JsonElement model, string path)
        {
            this.model = model;
            modelProperties = model.GetProperties2().ToList();
            foreach (var property in modelProperties)
            {
                properties[property.Name] = new JsonVersionedProperty(path + "/" + property.Name, property.Value);
            }

            modelObjects = model.GetObjects2().ToList();
            foreach (var o in modelObjects)
            {
                var obj = new JsonVersionedObject(o.Value, path + "/" + o.Name);
                objects[o.Name] = obj;
            }
        }

        public bool IsChanged =>
            properties.Values.Any(p => p.IsChanged)
            || objects.Values.Any(o => o.IsChanged);
        
        public JsonVersionedObject(JsonElement model) : this(model, string.Empty)
        {
        }

        public void AddVersion(string versionName, JsonElement versionElement)
        {
            // TODO: prevent adding the same version multiple times
            foreach (var mp in modelProperties)
            {
                var property = properties[mp.Name];
                var value = versionElement.GetStringProperty2(mp.Name);
                property.AddVersion(versionName, value);
            }

            foreach (var mo in modelObjects)
            {
                var obj = objects[mo.Name];
                var value = versionElement.GetObjectProperty2(mo.Name);
                obj.AddVersion(versionName, value);
            }
        }
    }

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