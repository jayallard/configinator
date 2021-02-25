using System;

namespace Allard.Configinator
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
    }
}