using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record AddedSchemaTypeToOrganizationEvent(
        OrganizationId OrganizationId,
        SchemaType SchemaType) : DomainEvent;
}