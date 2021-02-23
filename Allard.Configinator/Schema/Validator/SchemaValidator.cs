using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Text.Json;
using Allard.Configinator.Schema.Validator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Schema
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

            ValidateObject(validationErrors, document, type);
            return validationErrors;
        }

        private void ValidateObject(List<TypeValidationError> errors, JToken obj, SchemaParser.ObjectSchemaType type)
        {
            foreach (var p in type.Properties)
            {
                switch (p)
                {
                    case PropertyPrimitive prim:
                    {
                        var validator = validatorFactory.GetValidator(type.SchemaTypeId);
                        var jsonProperty = ((JProperty) obj)[p.Name];
                        if (jsonProperty == null)
                        {
                            errors.Add(new TypeValidationError("none", p.Name, "property does not exit"));
                            continue;
                        }

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