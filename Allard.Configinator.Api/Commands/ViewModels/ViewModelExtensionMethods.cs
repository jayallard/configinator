using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public static class ViewModelExtensionMethods
    {
        public static ConfigurationSectionViewModel ToConfigurationSectionViewModel(this ConfigurationSection section,
            LinkHelper linkHelper, bool self = false)
        {
            return new()
            {
                Name = section.Id.Name,
                Path = section.Path,
                TypeId = section.Type.SchemaTypeId.FullId,
                Links = linkHelper
                    .CreateBuilder()
                    .AddTypeId(section.Type.SchemaTypeId.FullId)
                    .AddConfigurationSection(section.Id, self)
                    .Build()
            };
        }

        public static RealmViewModel ToRealmViewModel(this Realm realm, LinkHelper linkHelper)
        {
            return new()
            {
                Name = realm.Name,
                ConfigurationSections = realm
                    .ConfigurationSections
                    .Select(cs => cs.ToConfigurationSectionViewModel(linkHelper))
                    .ToList(),
                Links = linkHelper
                    .CreateBuilder()
                    .Add("self", HttpMethod.Get, "realms", realm.Name)
                    .Build()
            };
        }

        public static SchemaTypeViewModel ToSchemaTypeViewModel(this ObjectSchemaType type, LinkHelper helper)
        {
            return new()
            {
                Properties = type.Properties.ToPropertyDtos(helper),
                TypeId = type.SchemaTypeId.FullId,
                Links = helper
                    .CreateBuilder()
                    .AddTypeId(type.SchemaTypeId.FullId)
                    .Build()
            };
        }

        public static IEnumerable<PropertyViewModel> ToPropertyDtos(this IEnumerable<Property> properties, LinkHelper helper)
        {
            return properties.Select(p => p.ToPropertyViewModel(helper));
        }

        public static PropertyViewModel ToPropertyViewModel(this Property property, LinkHelper helper)
        {
            var result = new PropertyViewModel
            {
                IsRequired = property.IsRequired,
                Name = property.Name,
                TypeId = property.TypeId.FullId
            };            
            
            if (property is PropertyPrimitive prim)
            {
                return result;
            }

            var group = (PropertyGroup) property;
            result.Properties = group.Properties.ToPropertyDtos(helper);
            return result;
        }
    }
}