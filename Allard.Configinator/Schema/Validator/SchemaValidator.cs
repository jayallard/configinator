using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Schema.Validator
{
    public class SchemaValidator
    {
        private readonly ITypeValidatorFactory validatorFactory;
        private readonly SchemaParser parser;

        public SchemaValidator(ITypeValidatorFactory validatorFactory, SchemaParser parser)
        {
            this.validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public async Task<List<TypeValidationError>> Validate(JToken document, SchemaParser.ObjectSchemaType type)
        {
            document = document ?? throw new ArgumentNullException(nameof(document));
            type = type ?? throw new ArgumentNullException(nameof(type));

            // TODO: object validations that have access to the properties. start with primitives only.
            type = type ?? throw new ArgumentNullException(nameof(type));
            var validationErrors = new List<TypeValidationError>();

            await ValidateObject(validationErrors, document, type, "/");
            return validationErrors;
        }

        private async Task ValidateObject(
            List<TypeValidationError> errors,
            JToken token,
            SchemaParser.ObjectSchemaType type,
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
                if (!HasValue(errors, obj, property, path))
                {
                    continue;
                }

                switch (property)
                {
                    case PropertyPrimitive prim:
                    {
                        var propPath = path + (path.Length == 1 ? "@" : "/@") + property.Name;
                        var validator = validatorFactory.GetValidator(type.SchemaTypeId);
                        if (validator == null)
                        {
                            continue;
                        }

                        // todo: validator
                        continue;
                    }
                    case PropertyGroup group:
                        var objPath = path + (path.Length == 1 ? string.Empty : "/") + property.Name;
                        var objType = await parser.GetSchemaType(property.TypeId.FullId);
                        await ValidateObject(errors, obj[property.Name], objType, objPath);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown property type: " + property.GetType().FullName);
                }
            }
        }

        private static bool HasValue(List<TypeValidationError> errors, JObject obj, Property property, string path)
        {
            // make sure the property exists.
            if (!obj.ContainsKey(property.Name))
            {
                if (property.IsRequired)
                {
                    errors.AddCoreError(path, "Required property doesn't exist: " + property.Name);
                }

                return false;
            }

            // if required, make sure the property has a value.
            var value = obj[property.Name];
            if (value.Type != JTokenType.Null)
            {
                return true;
            }

            if (property.IsRequired)
            {
                errors.AddCoreError(path,
                    "Required property doesn't exists but doesn't have a value: " + property.Name);
            }

            return false;
        }

        private IEnumerable<TypeValidationError> ValidateProperties(List<TypeValidationError> errors, JsonToken json,
            Property p)
        {
            return new List<TypeValidationError>();
        }
    }
}