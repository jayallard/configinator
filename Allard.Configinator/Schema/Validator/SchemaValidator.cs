using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Schema.Validator
{
    public class SchemaValidator : ISchemaValidator
    {
        private readonly ISchemaService service;
        private readonly ITypeValidatorFactory validatorFactory;

        public SchemaValidator(ITypeValidatorFactory validatorFactory, ISchemaService service)
        {
            this.validatorFactory = validatorFactory.EnsureValue(nameof(validatorFactory));
            this.service = service.EnsureValue(nameof(service));
        }

        public async Task<IList<TypeValidationError>> Validate(JToken document, ObjectSchemaType type)
        {
            document.EnsureValue(nameof(document));
            type.EnsureValue(nameof(type));

            var validationErrors = new List<TypeValidationError>();
            await ValidateObject(validationErrors, document, type, "/").ConfigureAwait(false);
            return validationErrors;
        }

        private async Task ValidateObject(
            List<TypeValidationError> errors,
            JToken token,
            ObjectSchemaType type,
            string path)
        {
            if (token is not JObject obj)
            {
                errors.AddCoreError(path, "Token should be " + JTokenType.Object + ", but is: " + token.Type);
                return;
            }

            // todo: properties in json that aren't in type
            foreach (var property in type.Properties)
            {
                if (!HasValue(errors, obj, property, path)) continue;

                switch (property)
                {
                    case PropertyPrimitive prim:
                    {
                        var propPath = path + (path.Length == 1 ? "@" : "/@") + property.Name;
                        var validator = validatorFactory.GetValidator(type.SchemaTypeId);
                        if (validator == null) continue;

                        // todo: validator
                        continue;
                    }
                    case PropertyGroup group:
                        var objPath = path + (path.Length == 1 ? string.Empty : "/") + property.Name;
                        var objType = await service.GetSchemaTypeAsync(property.TypeId.FullId).ConfigureAwait(false);
                        await ValidateObject(errors, obj[property.Name], objType, objPath).ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown property type: " + property.GetType().FullName);
                }
            }
        }

        private static bool HasValue(List<TypeValidationError> errors, JObject obj, Property property, string path)
        {
            // make sure the property exists.
            if (!obj.TryGetValue(property.Name, out var value))
            {
                if (property.IsRequired) errors.AddCoreError(path, "Required property doesn't exist: " + property.Name);
                return false;
            }

            // if required, make sure the property has a value.
            if (value.Type != JTokenType.Null) return true;
            if (property.IsRequired)
                errors.AddCoreError(path,
                    "Required property doesn't exists but doesn't have a value: " + property.Name);

            return false;
        }
    }
}