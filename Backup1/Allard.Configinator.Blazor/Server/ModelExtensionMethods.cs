using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Blazor.Server
{
    public static class ModelExtensionMethods
    {
        public static ConfigurationSectionViewModel ToViewModel(this ConfigurationSection configurationSection)
        {
            return new()
            {
                SectionId = configurationSection.SectionId.Id,
                RealmId = configurationSection.Realm.RealmId.Id,
                Properties = configurationSection.Properties.Select(p => p.ToViewModel()).ToList(),
                OrganizationId = configurationSection.Realm.Organization.OrganizationId.Id
            };
        }

        public static IEnumerable<ConfigurationSectionViewModel> ToViewModel(
            this IEnumerable<ConfigurationSection> configurationSections)
        {
            return configurationSections.Select(cs => cs.ToViewModel());
        }

        public static OrganizationViewModel ToViewModel(this OrganizationAggregate organization)
        {
            return new()
            {
                OrganizationId = organization.OrganizationId.Id,
                Realms = organization.Realms.ToViewModel().ToList(),
                SchemaTypes = organization.SchemaTypes.Select(t => t.ToViewModel()).ToList()
            };
        }

        public static IEnumerable<RealmViewModel> ToViewModel(this IEnumerable<Realm> realms)
        {
            return realms.Select(r => r.ToViewModel()).ToList();
        }

        public static RealmViewModel ToViewModel(this Realm realm)
        {
            return new()
            {
                RealmId = realm.RealmId.Id,
                Habitats = realm
                    .Habitats
                    .Select(h => new HabitatViewModel
                    {
                        HabitatId = h.HabitatId.Id,
                        BaseHabitatId = h.BaseHabitat.HabitatId.Id
                    })
                    .ToList(),
                ConfigurationSections = realm
                    .ConfigurationSections
                    .ToViewModel()
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

        public static IEnumerable<PropertyViewModel> ToViewModel(this IEnumerable<SchemaTypeProperty> properties)
        {
            return properties.Select(p => p.ToViewModel());
        }

        public static PropertyViewModel ToViewModel(this SchemaTypeProperty schemaTypeProperty)
        {
            return new()
            {
                IsRequired = schemaTypeProperty.IsRequired,
                IsSecret = schemaTypeProperty.IsSecret,
                Name = schemaTypeProperty.Name,
                SchemaTypeId = schemaTypeProperty.SchemaTypeId.FullId
            };
        }

        /*public static SchemaTypesViewModel ToViewModel(this IEnumerable<SchemaType> schemaTypes)
        {
            return new()
            {
                SchemaTypes = schemaTypes.Select(schemaType => schemaType.ToViewModel()).ToList()
            };
        }*/
    }
}