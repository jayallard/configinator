using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record RealmCreatedEvent(OrganizationId OrganizationId, RealmId RealmId) : DomainEvent;
}