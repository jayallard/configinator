using Allard.Configinator.Core.Ddd;

namespace Allard.Configinator.Core.Events
{
    public record OrganizationCreatedEvent(OrganizationId Id) : DomainEvent
    {
    }
}