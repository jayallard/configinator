using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public static class ExtensionMethods
    {
        public static Node ToObjectDto(this VersionedNode obj)
        {
            return new Node()
                .SetName(obj.Name)
                .Add(obj.Properties.ToPropertyDtos())
                .Add(obj.Objects.ToObjectDtos());
        }

        private static IEnumerable<Node> ToObjectDtos(this IEnumerable<VersionedNode> objects)
        {
            return objects == null
                ? new List<Node>()
                : objects.Select(ToObjectDto);
        }

        private static IEnumerable<Node> ToPropertyDtos(this IEnumerable<VersionedProperty> properties)
        {
            return properties == null
                ? new List<Node>()
                : properties.Select(p => Node.CreateString(p.Name, p.Value));
        }

        public static JsonDocument ToJson(this Node obj)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();
            WriteItems(writer, obj.Items);
            writer.WriteEndObject();
            writer.Flush();
            stream.Flush();
            stream.Position = 0;
            return JsonDocument.Parse(stream);
        }

        private static void WriteItems(Utf8JsonWriter writer, IEnumerable<Node> items)
        {
            foreach (var obj in items)
            {
                if (obj.IsProperty())
                {
                    writer.WriteString(obj.Name, obj.Value);
                    continue;
                }

                writer.WriteStartObject(obj.Name);
                WriteItems(writer, obj.Items);
                writer.WriteEndObject();
            }
        }

        public static bool IsProperty(this Node obj)
        {
            return obj.NodeType == NodeType.String;
        }

        public static bool IsObject(this Node obj)
        {
            return obj.NodeType == NodeType.Object;
        }
    }
}