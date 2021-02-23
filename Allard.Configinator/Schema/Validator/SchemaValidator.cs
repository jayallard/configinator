using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core.Tokens;

namespace Allard.Configinator.Schema.Validator
{
    public class SchemaValidator
    {
        private readonly ITypeValidatorFactory validatorFactory;

        public SchemaValidator(ITypeValidatorFactory validatorFactory)
        {
            this.validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
        }

        public List<TypeValidationError> Validate(JToken document, SchemaParser.ObjectSchemaType type)
        {
            // TODO: object validations that have access to the properties. start with primitives only.
            type = type ?? throw new ArgumentNullException(nameof(type));
            var validationErrors = new List<TypeValidationError>();

            ValidateObject(validationErrors, document, type, "/");
            return validationErrors;
        }

        private void ValidateObject(
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
                switch (p)
                {
                    case PropertyPrimitive prim:
                    {
                        path = path + (path.Length == 1 ? "@" : "/@") + p.Name;
                        
                        // make sure the property exists.
                        if (!obj.ContainsKey(p.Name))
                        {
                            errors.AddCoreError(path, "Required property doesn't exist: " + p.Name);
                            continue;
                        }
                        
                        var validator = validatorFactory.GetValidator(type.SchemaTypeId);
                        
                        // if (jsonProperty == null)
                        // {
                        //     errors.Add(new TypeValidationError("none", p.Name, "property does not exit"));
                        //     continue;
                        // }

                        continue;
                    }
                    case PropertyGroup group:
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