using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class JsonVersionedProperty
    {
        private readonly Dictionary<string, JsonVersionedPropertyValue> values = new();
        private readonly List<JsonVersionedPropertyValue> valuesOrdered = new();

        public JsonVersionedProperty(string objectPath)
        {
            ObjectPath = objectPath.EnsureValue(nameof(objectPath));
        }

        public string ObjectPath { get; }
        public JsonVersionedProperty Property { get; }
        public IReadOnlyCollection<JsonVersionedPropertyValue> Versions => valuesOrdered.AsReadOnly();

        public bool IsChanged => values.Values.Any(v => v.IsChanged);

        public void AddVersion(string version, JsonElement value)
        {
            version.EnsureValue(nameof(version));
            var v = new JsonVersionedPropertyValue(version, value, this);
            if (valuesOrdered.Count > 0)
            {
                v.PreviousValue = valuesOrdered.Last();
                valuesOrdered.Last().NextValue = v;
            }

            values[version] = v;
            valuesOrdered.Add(v);
        }

        public void UpdateVersion(string version, JsonElement value)
        {
            version.EnsureValue(nameof(version));
            values[version].SetValue(value);
        }

        public JsonVersionedPropertyValue GetValue(string versionName)
        {
            return values[versionName.EnsureValue(nameof(versionName))];
        }
    }
}