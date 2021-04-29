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

        public IEnumerable<ValidationFailure> Validate(HabitatId habitatId, Node value)
        {
            habitatId.EnsureValue(nameof(habitatId));
            value.EnsureValue(nameof(value));
            var results = new List<ValidationFailure>();
            Validate(results, habitatId, configurationSection.Properties.ToList(), value, string.Empty);
            return results;
        }

        private void Validate(
            ICollection<ValidationFailure> errors,
            HabitatId habitatId,
            IEnumerable<SchemaTypeProperty> properties,
            Node obj,
            string path)
        {
            // TODO: only works if all expected objects and properties exist.
            // needs to be hardened. objects and properties might not exist.
            var configId = new ConfigurationId(
                configurationSection.Realm.Organization.OrganizationId.Id,
                configurationSection.Realm.RealmId.Id,
                configurationSection.SectionId.Id,
                habitatId.Id);
            foreach (var property in properties)
            {
                var propertyPath = path + "/" + property.Name;
                if (property.SchemaTypeId.IsPrimitive)
                {
                    // property
                    var value = obj.GetProperty(property.Name).Value;
                    if (string.IsNullOrWhiteSpace(value) && property.IsRequired)
                        errors.Add(new ValidationFailure(configId, propertyPath,
                            "RequiredValueMissing", "A value is required."));

                    continue;
                }

                var schemaType = schemas[property.SchemaTypeId];
                var valueObject = obj.GetObject(property.Name);
                Validate(errors, habitatId, schemaType.Properties.ToList(), valueObject, propertyPath);
            }
        }
    }
}