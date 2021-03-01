using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public static class ViewModelExtensionMethods
    {
        public static ConfigurationSectionViewModel
            ToConfigurationSectionViewModel(this ConfigurationSection section, string realmName)
        {
            return new()
            {
                Name = section.Id.Name,
                Path = section.Path,
                Realm = realmName,
                TypeId = section.Type.SchemaTypeId.FullId
            };
        }

        public static RealmViewModel ToRealmViewModel(this Realm realm)
        {
            return new()
            {
                Name = realm.Name,
                ConfigurationSections = realm
                    .ConfigurationSections
                    .Select(cs => cs.ToConfigurationSectionViewModel(realm.Name))
                    .ToList()
            };
        }

        public static SchemaTypeViewModel ToSchemaTypeViewModel(this ObjectSchemaType type)
        {
            return new()
            {
                Properties = type.Properties.ToPropertyViewModels(),
                TypeId = type.SchemaTypeId.FullId
            };
        }

        public static IEnumerable<PropertyViewModel> ToPropertyViewModels(this IEnumerable<Property> properties)
        {
            return properties.Select(p => p.ToPropertyViewModel());
        }

        public static PropertyViewModel ToPropertyViewModel(this Property property)
        {
            var result = new PropertyViewModel
            {
                IsRequired = property.IsRequired,
                Name = property.Name,
                TypeId = property.SchemaType.SchemaTypeId.FullId
            };

            if (property is PropertyPrimitive) return result;

            var group = (PropertyGroup) property;
            result.Properties = group.Properties.ToPropertyViewModels();
            return result;
        }
    }
}