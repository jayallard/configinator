using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    public class SchemaTypeExploder
    {
        private readonly Dictionary<SchemaTypeId, SchemaTypeExploded> schemaTypes;

        public SchemaTypeExploder(IEnumerable<SchemaTypeExploded> schemaTypes)
        {
            this.schemaTypes = schemaTypes
                .EnsureValue(nameof(schemaTypes))
                .ToDictionary(s => s.SchemaTypeId);
        }

        public SchemaTypeExploded Explode(SchemaType schemaType)
        {
            var properties = schemaType
                .Properties
                .Select(Explode);
            return new SchemaTypeExploded(schemaType.SchemaTypeId, properties.ToList().AsReadOnly());
        }

        public SchemaTypePropertyExploded Explode(SchemaTypeProperty property)
        {
            var childProperties = property.SchemaTypeId.IsPrimitive
                ? new List<SchemaTypePropertyExploded>().AsReadOnly()
                : schemaTypes[property.SchemaTypeId].Properties;
            return new SchemaTypePropertyExploded(property.Name, property.SchemaTypeId, childProperties,
                property.IsSecret, property.IsOptional);
        }
    }
}