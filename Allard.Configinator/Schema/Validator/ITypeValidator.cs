using System.Collections.Generic;

namespace Allard.Configinator.Schema.Validator
{
    public interface ITypeValidator
    {
        string ForSchemaType { get; }
        IEnumerable<TypeValidationError> Validate(Property p, object value);
    }
}