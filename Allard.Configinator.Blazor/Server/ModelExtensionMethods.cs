using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Blazor.Shared;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Blazor.Server
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
                    SectionId = cs.SectionId,
                    RealmId = cs.Realm.RealmId,
                    Path = cs.Path,
                    SchemaTypeId = cs.SchemaType.SchemaTypeId.FullId
                });
        }


        public static OrganizationViewModel ToViewModel(this OrganizationAggregate organization)
        {
            return new()
            {
                OrganizationId = organization.OrganizationId.Id,
                Realms = organization.Realms.ToViewModel().ToList()
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
                Habitats = realm.Habitats
                    .Select(h => new HabitatViewModel(
                        h.HabitatId.Id,
                        h.Bases.Select(b => b.HabitatId.Id).ToList().AsReadOnly())).ToList(),
                ConfigurationSections = realm
                    .ConfigurationSections
                    .Select(cs => new ConfigurationSectionViewModel
                    {
                        OrganizationId = cs.Realm.Organization.OrganizationId,
                        SectionId = cs.SectionId,
                        RealmId = realm.RealmId,
                        Path = cs.Path,
                        SchemaTypeId = cs.SchemaType.SchemaTypeId.FullId
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

        public static IEnumerable<PropertyViewModel> ToViewModel(this IEnumerable<SchemaTypeProperty> properties)
        {
            return properties.Select(p => p.ToViewModel());
        }

        public static PropertyViewModel ToViewModel(this SchemaTypeProperty schemaTypeProperty)
        {
            return new()
            {
                IsRequired = schemaTypeProperty.IsRequired,
                Name = schemaTypeProperty.Name,
                SchemaTypeId = schemaTypeProperty.SchemaTypeId.FullId
            };
        }

        public static SchemaTypesViewModel ToViewModel(this IEnumerable<SchemaType> schemaTypes)
        {
            return new(schemaTypes.Select(schemaType => schemaType.ToViewModel()), new List<Link>());
        }
    }
}