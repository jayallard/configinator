using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<ValidationFailure> Validate(SchemaTypeId schemaTypeId, IObjectNode obj)
        {
            return new Instance(schemaTypes).Validate(schemaTypeId, obj, string.Empty);
        }

        private class Instance
        {
            private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

            public Instance(Dictionary<SchemaTypeId, SchemaType> schemaTypes)
            {
                this.schemaTypes = schemaTypes;
            }

            public IEnumerable<ValidationFailure> Validate(SchemaTypeId schemaTypeId, IObjectNode obj, string path)
            {
                var schemaType = schemaTypes[schemaTypeId];

                // todo: extra properties
                var valueProps = obj
                    .GetPropertyValues()
                    .ToDictionary(p => p.Name);
                var valueObjects = obj
                    .GetObjectNodes()
                    .ToDictionary(o => o.Name);

                foreach (var schemaProperty in schemaType.Properties)
                {
                    // handle primitives.
                    if (schemaProperty.SchemaTypeId.IsPrimitive)
                    {
                        if (valueProps.TryGetValue(schemaProperty.Name, out var p))
                        {
                            // property exists. validate the value.
                            if (p.Value == null)
                                if (schemaProperty.IsRequired)
                                    yield return new ValidationFailure(path, "RequiredPropertyValueMissing",
                                        schemaProperty.Name);

                            continue;
                        }

                        // property doesn't exist. if required, go boom.
                        if (schemaProperty.IsRequired)
                            yield return new ValidationFailure(path, "RequiredPropertyMissing", schemaProperty.Name);

                        continue;
                    }

                    // todo: null object is handled. but the object may exist and have no properties.
                    // useless distinction, but add it to keep it in sync with the property validations.
                    // handle objects.
                    if (valueObjects.TryGetValue(schemaProperty.Name, out var o))
                    {
                        // object exists
                        foreach (var value in Validate(schemaProperty.SchemaTypeId, o, path + "/" + schemaProperty.Name)
                        ) yield return value;
                        continue;
                    }

                    // object doesn't exist
                    if (schemaProperty.IsRequired)
                        yield return new ValidationFailure(path, "RequiredObjectMissing", schemaProperty.Name);
                }
            }
        }
    }
}