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
                .AddProperties(obj.Properties.ToPropertyDtos())
                .AddObjects(obj.Objects.ToObjectDtos());
        }

        private static PropertyDto ToPropertyDto(this VersionedProperty property)
        {
            return new()
            {
                Name = property.Name,
                Value = property.Value
            };
        }

        private static IEnumerable<ObjectDto> ToObjectDtos(this IEnumerable<VersionedObject> objects)
        {
            return objects == null
                ? new List<ObjectDto>()
                : objects.Select(ToObjectDto);
        }

        private static IEnumerable<PropertyDto> ToPropertyDtos(this IEnumerable<VersionedProperty> properties)
        {
            return properties == null
                ? new List<PropertyDto>()
                : properties.Select(ToPropertyDto);
        }

        public static JsonDocument ToJson(this ObjectDto obj)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();
            WriteProperties(writer, obj.Properties);
            WriteObjects(writer, obj.Objects);
            writer.WriteEndObject();
            writer.Flush();
            stream.Flush();
            stream.Position = 0;
            //using var reader = new StreamReader(stream);
            //var json = reader.ReadToEnd();
            return JsonDocument.Parse(stream);
        }
        
        private static void WriteProperties(Utf8JsonWriter writer, IEnumerable<PropertyDto> properties)
        {
            foreach (var p in properties)
            {
                writer.WriteString(p.Name, p.Value);
            }
        }

        private static void WriteObjects(Utf8JsonWriter writer, IEnumerable<ObjectDto> objects)
        {
            foreach (var obj in objects)
            {
                writer.WriteStartObject(obj.Name);
                WriteProperties(writer, obj.Properties);
                WriteObjects(writer, obj.Objects);
                writer.WriteEndObject();
            }
        }
    }
}