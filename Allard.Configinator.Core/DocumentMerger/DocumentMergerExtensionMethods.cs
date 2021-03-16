using System.Collections.Generic;
using System.IO;
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

        public static JsonDocument ToJson(this IEnumerable<MergedProperty> properties)
        {
            return JsonDocument.Parse(properties.ToJsonString());
        }

        public static string ToJsonString(this IEnumerable<MergedProperty> properties)
        {
            using var buffer = new MemoryStream();
            using var writer = new Utf8JsonWriter(buffer);
            writer.WriteStartObject();
            WriteProperties(writer, properties);
            writer.WriteEndObject();
            writer.Flush();
            buffer.Position = 0;
            using var reader = new StreamReader(buffer);
            return reader.ReadToEnd();
        }

        private static void WriteProperties(Utf8JsonWriter writer, IEnumerable<MergedProperty> properties)
        {
            foreach (var property in properties)
            {
                if (property.Children.Count == 0)
                {
                    if (property.Property.Value == null)
                    {
                        writer.WriteNull(property.Property.Name);
                        continue;
                    }

                    writer.WriteString(property.Property.Name, (string) property.Property.Value);
                    continue;
                }

                writer.WriteStartObject(property.Property.Name);
                WriteProperties(writer, property.Children);
                writer.WriteEndObject();
            }
        }
    }
}