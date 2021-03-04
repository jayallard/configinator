using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record ConfigurationSectionCreatedEvent (
        OrganizationId OrganizationId,
        RealmId RealmId,
        ConfigurationSectionId ConfigurationSectionId,
        SchemaTypeId SchemaTypeId,
        string Path,
        string Description) : DomainEvent;
}