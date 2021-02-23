using System.Collections.Generic;

namespace Allard.Configinator.Schema.Validator
{
    public class StringValidator : ITypeValidator
    {
        public string ForSchemaType => "string";
        public IEnumerable<TypeValidationError> Validate(Property p, object value)
        {
            return new List<TypeValidationError>();
        }
    }
}