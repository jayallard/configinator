using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    /// <summary>
    /// Converts a schema type to a Json document model.
    /// The model is the structure of the json document
    /// that represents the schema. The structure is defined.
    /// Properties are defined per object.
    /// The value of each property is set to a dummy value of the proper
    /// type. IE: if its a string property, the value is set to "" so
    /// that things that consume it know that its a string.
    /// </summary>
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
                        // need a value of the proper type.
                        // currently only support strings.
                        // this is how the model parser will know
                        // what type of values to expect.
                        // ie; when we support int, set the value to 0
                        // so everything knows its an int.
                        writer.WriteString(property.Name, string.Empty);
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