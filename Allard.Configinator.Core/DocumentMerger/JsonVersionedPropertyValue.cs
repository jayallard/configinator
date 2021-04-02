using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class JsonVersionedPropertyValue
    {
        public JsonVersionedPropertyValue(string versionName, JsonElement valueElement,
            JsonVersionedProperty parentProperty)
        {
            VersionName = versionName.EnsureValue(nameof(versionName));
            OriginalElement = ValueElement = valueElement;
            OriginalValue = Value = GetValue(valueElement);
            ParentProperty = parentProperty.EnsureValue(nameof(parentProperty));
        }

        public bool Exists => ValueElement.ValueKind != JsonValueKind.Undefined;
        public string OriginalValue { get; }
        public JsonElement OriginalElement { get; }
        public JsonElement ValueElement { get; private set; }
        public string Value { get; private set; }
        public string VersionName { get; }
        public bool IsChanged => !string.Equals(Value, OriginalValue);
        public bool IsSet { get; private set; }
        public JsonVersionedPropertyValue PreviousValue { get; internal set; }
        public JsonVersionedPropertyValue NextValue { get; internal set; }
        public JsonVersionedProperty ParentProperty { get; }

        public void SetValue(JsonElement value)
        {
            IsSet = true;
            ValueElement = value;
            Value = GetValue(value);
        }

        private string GetValue(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Undefined
                ? null
                : ValueElement.GetString();
        }
    }
}