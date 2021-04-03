using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public static class ExtensionMethods
    {
        public static ObjectDto ToObjectDto(this JsonDocument json)
        {
            var properties = json
                .RootElement
                .GetProperties()
                .Select(p => p.ToPropertyDto());
            var objects =
                json
                    .RootElement
                    .GetObjects()
                    .Select(GetObject);

            return new ObjectDto()
                .SetName("root")
                .AddProperties(properties)
                .AddObjects(objects);
        }

        private static ObjectDto GetObject(JsonProperty json)
        {
            var jsonObjects = json.Value.GetObjects();
            var jsonProperties = json.Value.GetProperties();
            var objs = jsonObjects
                .Select(GetObject);
            var props = jsonProperties
                .Select(p => p.ToPropertyDto());
            return new ObjectDto()
                .SetName(json.Name)
                .AddProperties(props)
                .AddObjects(objs);
        }

        private static IEnumerable<JsonProperty> GetObjects(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.Object);
        }

        private static PropertyDto ToPropertyDto(this JsonProperty property)
        {
            return new PropertyDto().SetName(property.Name).SetValue(property.Value.GetString());
        }

        private static IEnumerable<JsonProperty> GetProperties(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.String);
        }
    }
}