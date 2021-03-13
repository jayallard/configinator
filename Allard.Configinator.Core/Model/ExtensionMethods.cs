using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    public static class ExtensionMethods
    {
        public static void EnsureIdDoesntExist(this IReadOnlyCollection<ModelMemberId> keys, ModelMemberId id)
        {
            if (keys.Contains(id))
                throw new InvalidOperationException(
                    $"{id.GetType().Name} already exists. Id={id.Id}");
        }

        public static List<T> AddIfNotNull<T>(this List<T> list, T item)
        {
            if (item == null) return list;

            list.Add(item);
            return list;
        }
    }
}