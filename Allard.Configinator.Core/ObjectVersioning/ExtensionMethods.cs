using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public static class ExtensionMethods
    {
        public static ObjectDto ToObjectDto(this VersionedObject obj)
        {
            return new ObjectDto()
                .SetName(obj.Name)
                .Add(obj.Properties.ToPropertyDtos())
                .Add(obj.Objects.ToObjectDtos());
        }

        private static IEnumerable<ObjectDto> ToObjectDtos(this IEnumerable<VersionedObject> objects)
        {
            return objects == null
                ? new List<ObjectDto>()
                : objects.Select(ToObjectDto);
        }

        private static IEnumerable<ObjectDto> ToPropertyDtos(this IEnumerable<VersionedProperty> properties)
        {
            return properties == null
                ? new List<ObjectDto>()
                : properties.Select(p => ObjectDto.CreateString(p.Name, p.Value));
        }

        public static JsonDocument ToJson(this ObjectDto obj)
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

        private static void WriteItems(Utf8JsonWriter writer, IEnumerable<ObjectDto> items)
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

        public static bool IsProperty(this ObjectDto obj)
        {
            return obj.ObjectType == ObjectType.String;
        }

        public static bool IsObject(this ObjectDto obj)
        {
            return obj.ObjectType == ObjectType.Object;
        }
    }
}