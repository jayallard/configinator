using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    public static class ExtensionMethods
    {
        public static void EnsureNameDoesntAlreadyExist(this IReadOnlyCollection<string> keys, ModelMemberId id)
        {
            if (keys.Contains(id.Name))
                throw new InvalidOperationException(
                    $"A {id.GetType().Name} with that name already exists. Name={id.Name}");
        }
    }
}