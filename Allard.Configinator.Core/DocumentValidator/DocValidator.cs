using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.DocumentValidator
{
    public class DocValidator
    {
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

        public DocValidator(IEnumerable<SchemaType> schemaTypes)
        {
            this.schemaTypes = schemaTypes
                .EnsureValue(nameof(schemaTypes))
                .ToDictionary(s => s.SchemaTypeId);
        }

        public IEnumerable<ValidationFailure> Validate(IList<SchemaTypeProperty> properties, JsonElement doc)
        {
            return new Instance(schemaTypes).Validate(properties, doc, string.Empty);
        }

        private class Instance
        {
            private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

            public Instance(Dictionary<SchemaTypeId, SchemaType> schemaTypes)
            {
                this.schemaTypes = schemaTypes;
            }

            private static readonly Dictionary<string, KeyValuePair<string, object>> emptyObjectDictionary = new();
            private static readonly Dictionary<string, KeyValuePair<string, JsonElement>> emptyJsonElementDictionary = new();

            public IEnumerable<ValidationFailure> Validate(IList<SchemaTypeProperty> properties, JsonElement doc,
                string path)
            {
                var jsonProperties =
                    doc.ValueKind == JsonValueKind.Undefined
                        ? emptyObjectDictionary
                        : doc
                            .GetPropertyValues()
                            .ToDictionary(p => p.Key);
                var jsonObjects =
                    doc.ValueKind == JsonValueKind.Undefined
                        ? emptyJsonElementDictionary
                        : doc
                            .GetObjectNodes()
                            .ToDictionary(o => o.Key);

                // iterate the schema properties
                foreach (var schemaProperty in properties)
                {
                    // handle primitives.
                    if (schemaProperty.SchemaTypeId.IsPrimitive)
                    {
                        if (jsonProperties.TryGetValue(schemaProperty.Name, out var p))
                        {
                            // property exists. validate the value.
                            if (p.Value == null)
                                if (schemaProperty.IsRequired)
                                    yield return new ValidationFailure(path + "/" + schemaProperty.Name,
                                        "RequiredPropertyValueMissing",
                                        schemaProperty.Name);

                            continue;
                        }

                        // property doesn't exist. if required, go boom.
                        if (schemaProperty.IsRequired)
                            yield return new ValidationFailure(path + "/" + schemaProperty.Name,
                                "RequiredPropertyMissing", schemaProperty.Name);

                        continue;
                    }

                    // it's an object. see if it exists in the json.
                    var objectExists = jsonObjects.TryGetValue(schemaProperty.Name, out var o);
                    var value = objectExists
                        ? o.Value
                        : new JsonElement();

                    var newPath = path = "/" + schemaProperty.Name;
                    // if (!objectExists && schemaProperty.IsRequired)
                    //     yield return new ValidationFailure(newPath, "RequiredObjectMissing", schemaProperty.Name);

                    var schemaType = schemaTypes[schemaProperty.SchemaTypeId];
                    var validationFailures = Validate(schemaType.Properties.ToList(), value,
                        newPath);
                    foreach (var fail in validationFailures)
                    {
                        yield return fail;
                    }
                }
            }
        }
    }
}