using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core
{
    public static class Guards
    {
        public static string EnsureValue(this string value, string parameterName)
        {
            return string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentNullException(parameterName)
                : value;
        }

        public static T EnsureValue<T>(this T value, string parameterName)
        {
            return value == null
                ? throw new ArgumentNullException(parameterName)
                : value;
        }


        public static string ToNormalizedMemberName(this string modelItemName, string parameterName)
        {
            // todo: regex validation. only letters, numbers and -.
            return modelItemName.EnsureValue(parameterName).Trim().ToLowerInvariant();
        }
    }
}