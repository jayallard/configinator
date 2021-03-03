using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Events;

namespace Allard.Configinator.Core.Model
{
    public class Realm
    {
        private readonly List<Habitat> habitats = new();
        
        internal OrganizationAggregate Organization { get; }
        public RealmId Id { get; }

        public IReadOnlyCollection<Habitat> Habitats => habitats.AsReadOnly();

        internal void AddHabitat(Habitat habitat)
        {
            habitats.Add(habitat);
        }

        public Habitat CreateHabitat(string name)
        {
            name = name.Trim();
            if (habitats.Any(h => h.Id.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidOperationException("A habitat of that name already exists.");
            }

            var id = new HabitatId(Guid.NewGuid().ToString(), name);
            return Organization.EventHandlerRegistry.Raise<HabitatCreatedEvent, Habitat>(new HabitatCreatedEvent(Organization.OrganizationId, Id, id));
        }

        public Realm(RealmId id, OrganizationAggregate organization)
        {
            this.Organization = organization;
            Id = id;
        }
    }

    public record RealmId(string Id, string Name);
}