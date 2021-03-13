using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public static class ExtensionMethods
    {
        public static IEnumerable<KeyValuePair<string, object>> GetPropertyValues(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return new List<KeyValuePair<string, object>>();
            return element
                    
                .EnumerateObject()

                // todo: other types    
                .Where(o => o.Value.ValueKind == JsonValueKind.String || o.Value.ValueKind == JsonValueKind.Null)
                .Select(o => new KeyValuePair<string, object>(o.Name, o.Value.GetString()));
        }
        
        public static IEnumerable<KeyValuePair<string, JsonElement>> GetObjectNodes(this JsonElement parent)
        {
            if (parent.ValueKind == JsonValueKind.Null) return new List<KeyValuePair<string, JsonElement>>();

            return parent
                .EnumerateObject()
                .Where(o => o.Value.ValueKind == JsonValueKind.Object || o.Value.ValueKind == JsonValueKind.Null)
                .Select(o => new KeyValuePair<string, JsonElement>(o.Name, o.Value));
        }
    }
}