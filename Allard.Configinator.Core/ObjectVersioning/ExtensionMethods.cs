using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public static class ExtensionMethods
    {
        public static ObjectDto ToDto(this VersionedObject obj)
        {
            return new ObjectDto()
                .SetName(obj.Name)
                .AddProperties(obj.Properties.ToDto())
                .AddObjects(obj.Objects.ToDto());
        }

        private static PropertyDto ToDto(this VersionedProperty property)
        {
            return new()
            {
                Name = property.Name,
                Value = property.Value
            };
        }

        private static IEnumerable<ObjectDto> ToDto(this IEnumerable<VersionedObject> objects)
        {
            return objects == null
                ? new List<ObjectDto>()
                : objects.Select(ToDto);
        }

        private static IEnumerable<PropertyDto> ToDto(this IEnumerable<VersionedProperty> properties)
        {
            return properties == null
                ? new List<PropertyDto>()
                : properties.Select(ToDto);
        }
    }
}