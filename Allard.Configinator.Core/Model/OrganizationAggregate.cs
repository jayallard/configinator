using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;

namespace Allard.Configinator.Core.Model
{
    public class OrganizationAggregate : Aggregate<OrganizationId>
    {
        private readonly Dictionary<string, Realm> realms = new();
        private readonly EventHandlerRegistry registry;
        internal EventHandlerRegistry EventHandlerRegistry => registry;
        public OrganizationId OrganizationId { get; private set; }

        public OrganizationAggregate(OrganizationId id) : this()
        {
            registry.Raise(new OrganizationCreatedEvent(id));
        }

        public IReadOnlyCollection<Realm> Realms => realms.Values;

        public Realm CreateRealm(string name)
        {
            return registry.Raise<RealmCreatedEvent, Realm>(
                new RealmCreatedEvent(OrganizationId,
                    new RealmId(Guid.NewGuid().ToString(), name.ToNormalizedMemberName(nameof(name)))));
        }

        private OrganizationAggregate()
        {
            registry = new EventHandlerRegistryBuilder()
                .Register<OrganizationCreatedEvent>(e => OrganizationId = e.OrganizationId)
                .Register<RealmCreatedEvent, Realm>(e =>
                {
                    var realm = new Realm(e.RealmId, this);
                    realms.Add(realm.RealmId.Name, realm);
                    return realm;
                })
                .Register<HabitatCreatedEvent, Habitat>(e =>
                {
                    var realm = realms[e.RealmId.Name];
                    var bases = realm.Habitats.Where(h => e.Bases.Contains(h.HabitatId));
                    var habitat = new Habitat(e.HabitatId, realm, bases);
                    realm.AddHabitat(habitat);
                    return habitat;
                })
                .Register<ConfigurationSectionCreatedEvent, ConfigurationSection>(e =>
                {
                    var realm = realms[e.RealmId.Name];
                    var configurationSection = new ConfigurationSection(e.ConfigurationSectionId, e.Path,
                        new SchemaType(), e.Description);
                    realm.AddConfigurationSection(configurationSection);
                    return configurationSection;
                })
                .Build();
        }
    }

    public record OrganizationId(string Id)
    {
        public static OrganizationId NewOrganizationId()
        {
            return new(Guid.NewGuid().ToString());
        }
    }
}