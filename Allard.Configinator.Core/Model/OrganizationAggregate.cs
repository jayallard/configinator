using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;

namespace Allard.Configinator.Core.Model
{
    public class OrganizationAggregate : Aggregate<OrganizationId>
    {
        private readonly List<Realm> realms = new();
        private readonly EventHandlerRegistry registry;
        internal EventHandlerRegistry EventHandlerRegistry => registry;
        public OrganizationId OrganizationId { get; private set; }

        public OrganizationAggregate(OrganizationId id) : this()
        {
            registry.Raise(new OrganizationCreatedEvent(id));
        }

        public IReadOnlyCollection<Realm> Realms => realms.AsReadOnly();

        public Realm CreateRealm(string name)
        {
            return registry.Raise<RealmCreatedEvent, Realm>(
                new RealmCreatedEvent(OrganizationId, new RealmId( Guid.NewGuid().ToString(), name)));
        }

        private OrganizationAggregate()
        {
            registry = new EventHandlerRegistryBuilder()
                .Register<OrganizationCreatedEvent>(e => { OrganizationId = e.OrganizationId; })
                .Register<RealmCreatedEvent, Realm>(e =>
                {
                    var realm = new Realm(e.RealmId, this);
                    realms.Add(realm);
                    return realm;
                })
                .Register<HabitatCreatedEvent, Habitat>(e =>
                {
                    var realm = realms.Single(r => r.Id == e.RealmId);
                    var habitat = new Habitat(realm, e.HabitatId);
                    realm.AddHabitat(habitat);
                    return habitat;
                })
                .Build();
        }
    }
    
    public record OrganizationId(string Id)
    {
        public static OrganizationId NewOrganizationId()
        {
            return new (Guid.NewGuid().ToString());
        }
    }
}