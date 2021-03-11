using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Api
{
    public static class ModelExtensionMethods
    {
        public static IEnumerable<ConfigurationSectionViewModel> ToViewModel(
            IEnumerable<ConfigurationSection> configurationSections)
        {
            return configurationSections
                .Select(cs => new ConfigurationSectionViewModel
                {
                    OrganizationId = cs.Realm.Organization.OrganizationId,
                    ConfigurationSectionId = cs.ConfigurationSectionId,
                    RealmId = cs.Realm.RealmId,
                    Path = cs.Path,
                    SchemaTypeId = cs.SchemaTypeId.FullId
                });
        }

        public static RealmsViewModel ToViewModel(this IEnumerable<Realm> realms)
        {
            return new()
            {
                Realms = realms.Select(r => r.ToViewModel()).ToList()
            };
        }

        public static RealmViewModel ToViewModel(this Realm realm)
        {
            return new()
            {
                RealmName = realm.RealmId.Name,
                Habitats = realm.Habitats
                    .Select(h => new HabitatViewModel(
                        h.HabitatId.Name,
                        h.Bases.Select(b => b.HabitatId.Name).ToList().AsReadOnly())).ToList(),
                ConfigurationSections = realm
                    .ConfigurationSections
                    .Select(cs => new ConfigurationSectionViewModel
                    {
                        OrganizationId = cs.Realm.Organization.OrganizationId,
                        ConfigurationSectionId = cs.ConfigurationSectionId,
                        RealmId = realm.RealmId,
                        Path = cs.Path,
                        SchemaTypeId = cs.SchemaTypeId.FullId
                    })
                    .ToList()
            };
        }

        public static SchemaTypeViewModel ToViewModel(this SchemaType type)
        {
            return new()
            {
                Properties = type.Properties.ToViewModel(),
                SchemaTypeId = type.SchemaTypeId.FullId
            };
        }

        public static IEnumerable<PropertyViewModel> ToViewModel(this IEnumerable<Property> properties)
        {
            return properties.Select(p => p.ToViewModel());
        }

        public static PropertyViewModel ToViewModel(this Property property)
        {
            return new()
            {
                IsRequired = property.IsRequired,
                Name = property.Name,
                SchemaTypeId = property.SchemaTypeId.FullId
            };
        }

        public static SchemaTypesViewModel ToViewModel(this IEnumerable<SchemaType> schemaTypes)
        {
            return new(schemaTypes.Select(schemaType => schemaType.ToViewModel()), new List<Link>());
        }
        
    }
}