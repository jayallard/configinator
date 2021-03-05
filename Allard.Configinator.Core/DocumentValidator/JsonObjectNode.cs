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
        private readonly JsonElement json;
        public string Name { get; }

        public JsonObjectNode(string name, JsonElement json)
        {
            Name = name;
            this.json = json.EnsureValue(nameof(json));
        }

        public IEnumerable<IPropertyNode> GetPropertyValues()
        {
            return json
                .EnumerateObject()

                // todo: other types    
                .Where(o => o.Value.ValueKind == JsonValueKind.String || o.Value.ValueKind == JsonValueKind.Null)
                .Select(o => new PropertyNode(o.Name, o.Value.GetString()));
        }

        public IEnumerable<IObjectNode> GetObjectNodes()
        {
            return json
                .EnumerateObject()
                .Where(o => o.Value.ValueKind == JsonValueKind.Object || o.Value.ValueKind == JsonValueKind.Null)
                .Select(o => new JsonObjectNode(o.Name, o.Value));
        }
    }
}