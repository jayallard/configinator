using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model.Validators
{
    public class SchemaTypeValidator
    {
        // todo: properties aren't validated.
        // in old code the only recognized primitive is string, and that's not fully baked.
        // need to define primitives and be able to extend them, then validate them.
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;
        private readonly SchemaType toValidate;

        public SchemaTypeValidator(SchemaType toValidate, IEnumerable<SchemaType> schemaTypes)
        {
            this.toValidate = toValidate.EnsureValue(nameof(toValidate));
            this.schemaTypes = schemaTypes
                .EnsureValue(nameof(schemaTypes))
                .ToDictionary(st => st.SchemaTypeId);
        }

        public void Validate()
        {
            EnsureBothNotEmpty(toValidate.Properties, toValidate.PropertyGroups, "/");
            foreach (var g in toValidate.PropertyGroups) Validate(g, "/" + g.Name);
        }

        private void Validate(PropertyGroup group, string path)
        {
            EnsureValidType(group.SchemaTypeId, path);
            EnsureBothNotEmpty(group.Properties, group.PropertyGroups, path + "/" + group.Name);
            foreach (var g in group.PropertyGroups) Validate(g, path + "/" + g.Name);
        }

        private void EnsureValidType(SchemaTypeId typeId, string path)
        {
            if (!schemaTypes.ContainsKey(typeId))
                throw new InvalidOperationException("Type doesn't exist. Property Path=" + path + ". Unknown Type=" +
                                                    typeId.FullId);
        }

        private static void EnsureBothNotEmpty(IEnumerable<Property> properties, IEnumerable<PropertyGroup> groups, string path)
        {
            if (properties.ToList().Count == 0 && groups.ToList().Count == 0)
            {
                throw new InvalidOperationException("No properties or property groups. Path=" + path);
            }
        }
    }
}