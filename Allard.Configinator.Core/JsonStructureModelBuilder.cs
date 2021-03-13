using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public class JsonStructureModelBuilder
    {
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

        public JsonStructureModelBuilder(IEnumerable<SchemaType> schemaTypes)
        {
            this.schemaTypes = schemaTypes.ToDictionary(s => s.SchemaTypeId);
        }

        public JsonDocument ToStructureModel(ConfigurationSection section)
        {
            return new Instance(schemaTypes).ToStructureModel(section);
        }

        private class Instance
        {
            private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

            public Instance(Dictionary<SchemaTypeId, SchemaType> schemaTypes)
            {
                this.schemaTypes = schemaTypes;
            }

            public JsonDocument ToStructureModel(ConfigurationSection section)
            {
                using var output = new MemoryStream();
                using var writer = new Utf8JsonWriter(output, new JsonWriterOptions {Indented = true});
                writer.WriteStartObject();
                WriteSchemaType(writer, section.SchemaType);
                writer.WriteEndObject();
                writer.Flush();
                output.Position = 0;
                using var reader = new StreamReader(output);
                var json = reader.ReadToEnd();
                return JsonDocument.Parse(json);
            }

            private void WriteSchemaType(Utf8JsonWriter writer, SchemaType schemaType)
            {
                foreach (var property in schemaType.Properties)
                {
                    if (property.SchemaTypeId.IsPrimitive)
                    {
                        writer.WriteNull(property.Name);
                        continue;
                    }

                    writer.WriteStartObject(property.Name);
                    WriteSchemaType(writer, schemaTypes[property.SchemaTypeId]);
                    writer.WriteEndObject();
                }
            }
        }
    }
}