using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentValidator
{
    /// <summary>
    /// Object crawler for System.Text.Json.JsonElement.
    /// </summary>
    public class JsonObjectNode : IObjectNode
    {
        private JsonElement Json { get; }
        public string Name { get; }

        public JsonObjectNode(string name, JsonElement json)
        {
            Name = name;
            Json = json.EnsureValue(nameof(json));
        }

        public IEnumerable<IPropertyNode> GetPropertyValues()
        {
            if (Json.ValueKind == JsonValueKind.Null)
            {
                return new List<IPropertyNode>();
            }
            
            return Json
                .EnumerateObject()

                // todo: other types    
                .Where(o => o.Value.ValueKind == JsonValueKind.String || o.Value.ValueKind == JsonValueKind.Null)
                .Select(o => new PropertyNode(o.Name, o.Value.GetString()));
        }

        public IEnumerable<IObjectNode> GetObjectNodes()
        {
            if (Json.ValueKind == JsonValueKind.Null)
            {
                return new List<IObjectNode>();
            }
            
            try
            {
                return Json
                    .EnumerateObject()
                    .Where(o => o.Value.ValueKind == JsonValueKind.Object || o.Value.ValueKind == JsonValueKind.Null)
                    .Select(o => new JsonObjectNode(o.Name, o.Value));
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}