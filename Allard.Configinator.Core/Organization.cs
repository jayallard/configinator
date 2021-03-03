using System;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;

namespace Allard.Configinator.Core
{
    public class Organization : Aggregate<OrganizationId>
    {
        private readonly EventHandlerRegistry registry;
        public OrganizationId Id { get; private set; }
        public Organization(OrganizationId id) : this()
        {
            registry.Raise(new OrganizationCreatedEvent(id));
        }

        public Realm CreateRealm(string Name)
        {
            return (Realm) registry.Raise(new RealmCreatedEvent(Name, Guid.NewGuid().ToString()));
        }
        
        
        private Organization()
        {
            registry = new EventHandlerRegistryBuilder()
                .Register<OrganizationCreatedEvent>(e =>
                {
                    var evt = (OrganizationCreatedEvent) e;
                    Id = evt.Id;
                })
                .Build();
        }
    }
}