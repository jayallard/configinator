using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core.DocumentValidator
{
    public class ConfigurationValidator
    {
        private readonly ConfigurationSection configurationSection;
        private readonly Dictionary<SchemaTypeId, SchemaType> schemas;

        public ConfigurationValidator(ConfigurationSection configurationSection, IEnumerable<SchemaType> schemaTypes)
        {
            this.configurationSection = configurationSection.EnsureValue(nameof(ConfigurationSection));
            schemas = schemaTypes.EnsureValue(nameof(schemaTypes)).ToDictionary(st => st.SchemaTypeId);
        }

        public IEnumerable<ValidationFailure> Validate(HabitatId habitatId, ObjectDto value)
        {
            var results = new List<ValidationFailure>();
            Validate(results, habitatId, configurationSection.Properties.ToList(), value, string.Empty);
            return results;
        }

        private void Validate(
            List<ValidationFailure> errors,
            HabitatId habitatId,
            List<SchemaTypeProperty> properties,
            ObjectDto obj,
            string path)
        {
            var configId = new ConfigurationId(
                configurationSection.Realm.Organization.OrganizationId.Id,
                configurationSection.Realm.RealmId.Id,
                configurationSection.SectionId.Id,
                habitatId.Id);
            foreach (var property in properties)
            {
                var propertyPath = path + "/" + property.Name;
                var schemaType = schemas[property.SchemaTypeId];
                if (schemaType.SchemaTypeId.IsPrimitive)
                {
                    // property
                    var value = obj.GetProperty(property.Name).Value;
                    if (string.IsNullOrWhiteSpace(value) && property.IsRequired)
                    {
                        errors.Add(new ValidationFailure(configId, propertyPath,
                            "RequiredValueMissing", "A value is required."));
                    }

                    continue;
                }

                var valueObject = obj.GetObject(property.Name);
                Validate(errors, habitatId, schemaType.Properties.ToList(), valueObject, propertyPath);
            }
        }
    }
}