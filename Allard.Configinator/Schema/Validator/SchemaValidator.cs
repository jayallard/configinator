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
            foreach (var p in type.Properties)
            {
                // make sure the property exists.
                if (!obj.ContainsKey(p.Name))
                {
                    errors.AddCoreError(path, "Required property doesn't exist: " + p.Name);
                    continue;
                }

                switch (p)
                {
                    case PropertyPrimitive prim:
                    {
                        var propPath = path + (path.Length == 1 ? "@" : "/@") + p.Name;
                        var validator = validatorFactory.GetValidator(type.SchemaTypeId);
                        if (validator == null)
                        {
                            continue;
                        }
                        
                        // todo: validator
                        continue;
                    }
                    case PropertyGroup group:
                        var objPath = path + (path.Length == 1 ? string.Empty : "/") + p.Name;
                        var objType = await parser.GetSchemaType(p.TypeId.FullId);
                        ValidateObject(errors, obj[p.Name], objType, objPath);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown property type: " + p.GetType().FullName);
                }
            }
        }

        private IEnumerable<TypeValidationError> ValidateProperties(List<TypeValidationError> errors, JsonToken json,
            Property p)
        {
            return new List<TypeValidationError>();
        }
    }
}