using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public class StructureBuilder
    {
        private readonly ConfigurationSection configurationSection;

        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

        private StructureBuilder(ConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection.EnsureValue(nameof(configurationSection));
            schemaTypes = configurationSection
                .Realm
                .Organization
                .SchemaTypes
                .ToDictionary(s => s.SchemaTypeId);
        }

        public static ObjectDto ToStructure(ConfigurationSection configurationSection)
        {
            return new StructureBuilder(configurationSection).Build();
        }

        private ObjectDto Build()
        {
            var obj = new ObjectDto();
            Build(obj, configurationSection.Properties);
            return obj;
        }

        private void Build(ObjectDto obj, IEnumerable<SchemaTypeProperty> properties)
        {
            foreach (var p in properties)
            {
                if (p.SchemaTypeId.IsPrimitive)
                {
                    var propertyDto = new PropertyDto {Name = p.Name};
                    obj.Properties.Add(propertyDto);
                    continue;
                }

                var type = schemaTypes[p.SchemaTypeId];
                var childObj = new ObjectDto().SetName(p.Name);
                obj.Objects.Add(childObj);
                Build(childObj, type.Properties);
            }
        }
    }
}