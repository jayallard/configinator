using System.Collections.Generic;
using Allard.Configinator.Schema.Validator;

namespace Allard.Configinator.Schema
{
    public interface ITypeValidator
    {
        string ForSchemaType { get; }
        IEnumerable<TypeValidationError> Validate(Property p, object value);
    }
}