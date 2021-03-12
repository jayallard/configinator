using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record AddedConfigurationSectionToRealmEvent (
        OrganizationId OrganizationId,
        RealmId RealmId,
        SectionId SectionId,
        SchemaTypeId SchemaTypeId,
        string Path,
        string Description) : DomainEvent;
}