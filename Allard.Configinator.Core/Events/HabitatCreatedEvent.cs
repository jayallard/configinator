using System.Collections.Generic;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Events
{
    public record HabitatCreatedEvent(
        OrganizationId OrganizationId, 
        RealmId RealmId,
        HabitatId HabitatId,
        IEnumerable<HabitatId> Bases) : DomainEvent;
}