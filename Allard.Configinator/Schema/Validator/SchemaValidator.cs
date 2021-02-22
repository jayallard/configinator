using System;
using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Schema.Validator;
using Newtonsoft.Json;

namespace Allard.Configinator.Schema
{
    public class SchemaValidator
    {
        private readonly ITypeValidatorFactory validatorFactory;

        public SchemaValidator(ITypeValidatorFactory validatorFactory)
        {
            this.validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
        }

        public List<TypeValidationError> Validate(JsonToken document, SchemaParser.ObjectSchemaType type)
        {
            // TODO: object validations that have access to the properties. start with primitives only.
            type = type ?? throw new ArgumentNullException(nameof(type));
            var validationErrors = new List<TypeValidationError>();

            ValidateObject(validationErrors, document, type);
        }

        private void ValidateObject(List<TypeValidationError> errors, JsonToken obj, SchemaParser.ObjectSchemaType type)
        {
            foreach (var p in type.Properties)
            {
                switch (p)
                {
                    case PropertyPrimitive prim:
                    {
                        var validator = validatorFactory.GetValidator(type.SchemaTypeId);
                        var value = 
                        continue;
                    }
                    case PropertyGroup group:
                        break;
                    default:
                        throw new InvalidOperationException("Unknown property type: " + p.GetType().FullName);
                }
            }
        }

        private IEnumerable<TypeValidationError> ValidateProperties(List<TypeValidationError> errors, JsonToken json, Property p)
        {
            
        } 
        
        
    }
}