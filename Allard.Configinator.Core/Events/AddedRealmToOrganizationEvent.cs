using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record AddedRealmToOrganizationEvent(OrganizationId OrganizationId, RealmId RealmId) : DomainEvent;
}