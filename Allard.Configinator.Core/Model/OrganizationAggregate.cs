using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model.Validators;

namespace Allard.Configinator.Core.Model
{
    public class OrganizationAggregate : Aggregate<OrganizationId>
    {
        private readonly Dictionary<string, Realm> realms = new();
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes = new();
        private readonly EventHandlerRegistry registry;
        internal EventHandlerRegistry EventHandlerRegistry => registry;
        public OrganizationId OrganizationId { get; private set; }

        public OrganizationAggregate(OrganizationId organizationId) : this()
        {
            organizationId.EnsureValue(nameof(organizationId));
            registry.Raise(new OrganizationCreatedEvent(organizationId));
        }

        public SchemaType GetSchema(SchemaTypeId schemaTypeId)
        {
            if (schemaTypes.TryGetValue(schemaTypeId, out var schemaType))
            {
                return schemaType;
            }

            throw new InvalidOperationException("The type doesn't exist in the organization: " + schemaTypeId.FullId);
        }

        public IReadOnlyCollection<Realm> Realms => realms.Values;
        public IReadOnlyCollection<SchemaType> SchemaTypes => schemaTypes.Values;

        public Realm AddRealm(string realmName)
        {
            var realmId = RealmId.NewRealmId(realmName);
            realms.Keys.EnsureNameDoesntAlreadyExist(realmId);
            return registry.Raise<AddedRealmToOrganizationEvent, Realm>(
                new AddedRealmToOrganizationEvent(OrganizationId,
                    new RealmId(Guid.NewGuid().ToString(), realmName.ToNormalizedMemberName(nameof(realmName)))));
        }

        public SchemaType AddSchemaType(SchemaType schemaType)
        {
            if (schemaTypes.ContainsKey(schemaType.SchemaTypeId))
            {
                throw new InvalidOperationException("Schema already exists");
            }

            new SchemaTypeValidator(schemaType, schemaTypes.Values).Validate();
            var evt = new AddedSchemaTypeToOrganizationEvent(this.OrganizationId, schemaType);
            return EventHandlerRegistry.Raise<AddedSchemaTypeToOrganizationEvent, SchemaType>(evt);
        }

        private OrganizationAggregate()
        {
            registry = new EventHandlerRegistryBuilder()
                // create org
                .Register<OrganizationCreatedEvent>(e => OrganizationId = e.OrganizationId)

                // add realm to organization
                .Register<AddedRealmToOrganizationEvent, Realm>(e =>
                {
                    var realm = new Realm(e.RealmId, this);
                    realms.Add(realm.RealmId.Name, realm);
                    return realm;
                })

                // add habitat to realm
                .Register<AddedHabitatToRealmEvent, Habitat>(e =>
                {
                    var realm = realms[e.RealmId.Name];
                    var bases = realm.Habitats.Where(h => e.Bases.Contains(h.HabitatId));
                    var habitat = new Habitat(e.HabitatId, realm, bases);
                    realm.AddHabitat(habitat);
                    return habitat;
                })

                // add configuration section to realm
                .Register<AddedConfigurationSectionToRealmEvent, ConfigurationSection>(e =>
                {
                    var realm = realms[e.RealmId.Name];
                    var configurationSection = new ConfigurationSection(e.ConfigurationSectionId, e.Path,
                        null, e.Description);
                    realm.AddConfigurationSection(configurationSection);
                    return configurationSection;
                })

                // add schema type to organization
                .Register<AddedSchemaTypeToOrganizationEvent, SchemaType>(e =>
                {
                    schemaTypes.Add(e.SchemaType.SchemaTypeId, e.SchemaType);
                    return e.SchemaType;
                })
                .Build();
        }
    }
}