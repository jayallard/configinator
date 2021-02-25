using System.Collections.Generic;

namespace Allard.Configinator.Schema.Validator
{
    public static class ExtensionMethods
    {
        public static void AddCoreError(this IList<TypeValidationError> errors, string path, string errorMessage)
        {
            AddError(errors, "Core", path, errorMessage);
        }

        public static void AddError(this IList<TypeValidationError> errors, string validatorName, string path,
            string errorMessage)
        {
            errors.Add(new TypeValidationError(validatorName, path, errorMessage));
        }
    }
}