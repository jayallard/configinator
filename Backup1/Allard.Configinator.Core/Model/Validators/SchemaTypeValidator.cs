using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model.Validators
{
    public class SchemaTypeValidator
    {
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes;
        private readonly SchemaType toValidate;

        public SchemaTypeValidator(SchemaType toValidate, IEnumerable<SchemaType> schemaTypes)
        {
            this.toValidate = toValidate.EnsureValue(nameof(toValidate));
            this.schemaTypes = schemaTypes
                .EnsureValue(nameof(schemaTypes))
                .ToDictionary(st => st.SchemaTypeId);
        }

        public static void Validate(SchemaType toValidate, IEnumerable<SchemaType> validateAgainst)
        {
            new SchemaTypeValidator(toValidate, validateAgainst).Validate();
        }

        public void Validate()
        {
            ValidateSchemaType(toValidate, string.Empty);
        }

        private void ValidateSchemaType(SchemaType schemaType, string path)
        {
            EnsureNotEmpty(toValidate.Properties, path);
            foreach (var property in schemaType.Properties) ValidateProperty(property, path);
        }

        private void ValidateProperty(SchemaTypeProperty schemaTypeProperty, string path)
        {
            if (schemaTypeProperty.SchemaTypeId.IsPrimitive) return;

            path = path + "/" + schemaTypeProperty.Name;
            EnsureNotCircular(schemaTypeProperty.SchemaTypeId, path);
            EnsureValidType(schemaTypeProperty.SchemaTypeId, path);
            ValidateSchemaType(schemaTypes[schemaTypeProperty.SchemaTypeId], path);
        }

        private void EnsureNotCircular(SchemaTypeId schemaTypeId, string path)
        {
            if (schemaTypeId == toValidate.SchemaTypeId)
                throw new InvalidOperationException("Circular reference. Path=" + path + ", SchemaTypeId=" +
                                                    schemaTypeId.FullId);
        }

        private void EnsureValidType(SchemaTypeId typeId, string path)
        {
            if (!schemaTypes.ContainsKey(typeId))
                throw new InvalidOperationException("Type doesn't exist. Property Path=" + path + ". Unknown Type=" +
                                                    typeId.FullId);
        }

        private static void EnsureNotEmpty(IEnumerable<SchemaTypeProperty> properties, string path)
        {
            if (properties.ToList().Count == 0)
                throw new InvalidOperationException("The SchemaType doesn't have any properties.. Path=" +
                                                    (path.Length == 0 ? "/" : path));
        }
    }
}