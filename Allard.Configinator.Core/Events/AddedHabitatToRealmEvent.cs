using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record AddedHabitatToRealmEvent(
        OrganizationId OrganizationId,
        RealmId RealmId,
        HabitatId HabitatId,
        HabitatId BaseHabitatId = null) : DomainEvent;
}