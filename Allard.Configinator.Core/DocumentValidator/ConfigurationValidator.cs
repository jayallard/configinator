using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core.DocumentValidator
{
    public class ConfigurationValidator
    {
        private readonly List<SchemaTypePropertyExploded> properties;

        public ConfigurationValidator(IEnumerable<SchemaTypePropertyExploded> properties)
        {
            this.properties = properties.EnsureValue(nameof(properties)).ToList();
        }

        public IEnumerable<SchemaValidationFailure> Validate(Node value)
        {
            value.EnsureValue(nameof(value));
            var results = new List<SchemaValidationFailure>();
            Validate(results, properties, value, string.Empty);
            return results;
        }

        private void Validate(
            ICollection<SchemaValidationFailure> errors,
            IEnumerable<SchemaTypePropertyExploded> props,
            Node obj,
            string path)
        {
            foreach (var property in props)
            {
                var propertyPath = path + "/" + property.Name;
                if (property.SchemaTypeId.IsPrimitive)
                {
                    // property
                    var value = obj.GetProperty(property.Name).Value;
                    if (string.IsNullOrWhiteSpace(value) && property.IsRequired)
                        errors.Add(new SchemaValidationFailure( propertyPath,
                            "RequiredValueMissing", "A value is required."));
                    continue;
                }

                var valueObject = obj.GetObject(property.Name);
                Validate(errors, property.Properties, valueObject, propertyPath);
            }
        }
    }
}