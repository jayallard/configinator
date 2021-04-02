using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.DocumentValidator
{
    public class ConfigurationValidator
    {
        private readonly ConfigurationSection configurationSection;
        private readonly Dictionary<SchemaTypeId, SchemaType> schemas;

        public ConfigurationValidator(ConfigurationSection configurationSection, IEnumerable<SchemaType> schemaTypes)
        {
            configurationSection = this.configurationSection.EnsureValue(nameof(ConfigurationSection));
            schemas = schemaTypes.EnsureValue(nameof(schemaTypes)).ToDictionary(st => st.SchemaTypeId);
        }

        public IEnumerable<ValidationFailure> Validate(HabitatId habitatId, JsonVersionedObject value)
        {
            var results = new List<ValidationFailure>();
            Validate(results, habitatId, configurationSection.Properties.ToList(), value);
            return results;
        }

        private void Validate(
            List<ValidationFailure> errors,
            HabitatId habitatId,
            List<SchemaTypeProperty> properties,
            JsonVersionedObject obj)
        {
            foreach (var property in properties)
            {
                var schemaType = schemas[property.SchemaTypeId];
                if (schemaType.SchemaTypeId.IsPrimitive)
                {
                    // property
                    var valueProperty = obj.GetProperty(property.Name).GetValue(habitatId.Id);
                    if (string.IsNullOrWhiteSpace(valueProperty.Value) && property.IsRequired)
                    {
                        var configId = new ConfigurationId(
                            configurationSection.Realm.Organization.OrganizationId.Id,
                            configurationSection.Realm.RealmId.Id,
                            configurationSection.SectionId.Id,
                            habitatId.Id);
                        errors.Add(new ValidationFailure(configId, valueProperty.ParentProperty.ObjectPath,
                            "RequiredValueMissing", "A value is required."));
                    }

                    continue;
                }

                var valueObject = obj.GetObject(property.Name);
                Validate(errors, habitatId, schemaType.Properties.ToList(), valueObject);
            }
        }
    }
}