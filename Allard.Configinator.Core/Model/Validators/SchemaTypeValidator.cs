using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Allard.Configinator.Core.Model.Validators
{
    public class SchemaTypeValidator
    {
        private readonly SchemaType toValidate;
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;

        public SchemaTypeValidator(SchemaType toValidate, IEnumerable<SchemaType> schemaTypes)
        {
            this.toValidate = toValidate.EnsureValue(nameof(toValidate));
            this.schemaTypes = schemaTypes
                .EnsureValue(nameof(schemaTypes))
                .ToDictionary(st => st.SchemaTypeId);
        }

        public void Validate()
        {
            if (toValidate.Properties.Count == 0 && toValidate.PropertyGroups.Count == 0)
            {
                throw new InvalidOperationException("The schema type doesn't have any properties or property groups");
            }
            
            foreach (var group in toValidate.PropertyGroups)
            {
                Validate(group, "/" + group.Name);
            }
        }

        private void Validate(PropertyGroup group, string path)
        {
            if (!schemaTypes.ContainsKey(group.SchemaTypeId))
            {
                throw new InvalidOperationException("Type doesn't exist. Property Path=" + path + ". Unknown Type=" + group.SchemaTypeId.FullId);
            }

            if (group.Properties.Count == 0)
            {
                throw new InvalidOperationException("Property group doesn't have any properties: " + path);
            }

            foreach (var g in group.PropertyGroups)
            {
                Validate(g, path + "/" + g.Name);
            }
        }
    }
}