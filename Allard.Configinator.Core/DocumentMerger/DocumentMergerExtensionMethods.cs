using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public static class DocumentMergerExtensionMethods
    {
        /// <summary>
        ///     Returns true if the transition is any variation
        ///     of SET.
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        public static bool IsSet(this Transition transition)
        {
            return transition == Transition.Set
                   || transition == Transition.SetToSameValue;
        }

        public static string ToJsonString(this ObjectValue obj)
        {
            using var buffer = new MemoryStream();
            using var writer = new Utf8JsonWriter(buffer);
            writer.WriteStartObject();
            WriteProperties(writer, obj.Properties);
            WriteObjects(writer, obj.Objects);
            writer.WriteEndObject();
            writer.Flush();
            buffer.Position = 0;
            using var reader = new StreamReader(buffer);
            return reader.ReadToEnd();
        }

        private static void WriteObjects(Utf8JsonWriter writer, IEnumerable<ObjectValue> objects)
        {
            foreach (var o in objects)
            {
                writer.WriteStartObject(o.Name);
                WriteProperties(writer, o.Properties);
                WriteObjects(writer, o.Objects);
                writer.WriteEndObject();
            }
        }

        private static void WriteProperties(Utf8JsonWriter writer, IEnumerable<PropertyValue> properties)
        {
            foreach (var property in properties)
            {
                writer.WriteString(property.Name, property.Value);
            }
        }

        public static IEnumerable<JsonProperty> GetObjects(this JsonElement element)
        {
            return element.EnumerateObject().Where(e => e.Value.ValueKind == JsonValueKind.Object);
        }
    }
}