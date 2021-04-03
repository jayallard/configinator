using System;
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

        public static IEnumerable<JsonProperty> GetObjects(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.Object);
        }

        public static PropertyDto ToPropertyDto(this JsonProperty property)
        {
            return new PropertyDto().SetName(property.Name).SetValue(property.Value.GetString());
        }

        public static IEnumerable<JsonProperty> GetProperties(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.String);
        }

        public static JsonElement GetStringProperty(this JsonElement parentElement, string propertyName)
        {
            if (parentElement.ValueKind == JsonValueKind.Undefined) return parentElement;
            if (parentElement.TryGetProperty(propertyName, out var existing))
                return existing.ValueKind == JsonValueKind.String
                    ? existing
                    : default;

            return default;
        }

        public static JsonElement GetObjectProperty(this JsonElement parentElement, string propertyName)
        {
            if (parentElement.ValueKind == JsonValueKind.Undefined) return parentElement;
            if (parentElement.TryGetProperty(propertyName, out var existing))
                return existing.ValueKind == JsonValueKind.Object
                    ? existing
                    : default;

            return default;
        }

        public static void Visit<TId, TValue>(this Tree<TId, TValue>.Leaf<TId, TValue> leaf,
            Action<Tree<TId, TValue>.Leaf<TId, TValue>> visit) where TId : class
        {
            visit(leaf);
            foreach (var child in leaf.Children) child.Visit(visit);
        }
    }
}