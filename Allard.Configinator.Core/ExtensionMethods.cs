using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core
{
    public static class ExtensionMethods
    {
        public static IEnumerable<JsonProperty> GetObjects(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.Object);
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