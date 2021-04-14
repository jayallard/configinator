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

        public static Node ToStructure(ConfigurationSection configurationSection)
        {
            return new StructureBuilder(configurationSection).Build();
        }

        private Node Build()
        {
            var obj = Node.CreateObject();
            Build(obj, configurationSection.Properties);
            return obj;
        }

        private void Build(Node obj, IEnumerable<SchemaTypeProperty> properties)
        {
            foreach (var p in properties)
            {
                if (p.SchemaTypeId.IsPrimitive)
                {
                    var propertyDto = Node.CreateString(p.Name);
                    obj.Add(propertyDto);
                    continue;
                }

                var type = schemaTypes[p.SchemaTypeId];
                var childObj = Node.CreateObject(p.Name);
                obj.Items.Add(childObj);
                Build(childObj, type.Properties);
            }
        }
    }
}