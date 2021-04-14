using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public static class ExtensionMethods
    {
        public static Node ToObjectDto(this JsonDocument json)
        {
            if (json.RootElement.ValueKind != JsonValueKind.Object)
                return Node.CreateString(null, json.RootElement.GetString());

            var properties = json
                .RootElement
                .GetProperties()
                .Select(p => Node.CreateString(p.Name, p.Value.GetString()));
            var objects =
                json
                    .RootElement
                    .GetObjects()
                    .Select(GetObject);

            return Node.CreateObject()
                .Add(properties)
                .Add(objects);
        }

        private static Node GetObject(JsonProperty json)
        {
            var jsonObjects = json.Value.GetObjects();
            var jsonProperties = json.Value.GetProperties();
            var objs = jsonObjects
                .Select(GetObject);
            var props = jsonProperties
                .Select(p => Node.CreateString(p.Name, p.Value.GetString()));
            return Node.CreateObject(json.Name)
                .Add(props)
                .Add(objs);
        }

        private static IEnumerable<JsonProperty> GetObjects(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.Object);
        }

        private static IEnumerable<JsonProperty> GetProperties(this JsonElement element)
        {
            return element.EnumerateObject().Where(e =>
                e.Value.ValueKind is JsonValueKind.String or JsonValueKind.Null);
        }
    }
}