using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model.Validators;

namespace Allard.Configinator.Core.Model
{
    public class OrganizationAggregate : IAggregate<OrganizationId>
    {
        private readonly Dictionary<RealmId, Realm> realms = new();
        private readonly Dictionary<SchemaTypeId, SchemaType> schemaTypes = new();

        public OrganizationAggregate(OrganizationId organizationId) : this()
        {
            organizationId.EnsureValue(nameof(organizationId));
            EventHandlerRegistry.Raise(new OrganizationCreatedEvent(organizationId));
        }

        private OrganizationAggregate()
        {
            EventHandlerRegistry = new EventHandlerRegistryBuilder()
                // create org
                .Register<OrganizationCreatedEvent>(e => OrganizationId = e.OrganizationId)

                // add realm to organization
                .Register<AddedRealmToOrganizationEvent, Realm>(e =>
                {
                    var realm = new Realm(this, e.RealmId);
                    realms.Add(realm.RealmId, realm);
                    return realm;
                })

                // add habitat to realm
                .Register<AddedHabitatToRealmEvent, Habitat>(e =>
                {
                    var realm = realms[e.RealmId];
                    var bases = realm.Habitats.Where(h => e.Bases.Contains(h.HabitatId));
                    var habitat = new Habitat(e.HabitatId, realm, bases);
                    realm.AddHabitat(habitat);
                    return habitat;
                })

                // add configuration section to realm
                .Register<AddedConfigurationSectionToRealmEvent, ConfigurationSection>(e =>
                {
                    var realm = realms[e.RealmId];
                    var configurationSection = new ConfigurationSection(
                        realm,
                        e.SectionId,
                        e.Path,
                        schemaTypes[e.SchemaTypeId],
                        e.Description);
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

        internal EventHandlerRegistry EventHandlerRegistry { get; }

        public OrganizationId OrganizationId { get; private set; }

        public IReadOnlyCollection<Realm> Realms => realms.Values;

        public IReadOnlyCollection<SchemaType> SchemaTypes => schemaTypes.Values;

        public Realm GetRealmById(string realmId)
        {
            var realm = realms
                .Values
                .SingleOrDefault(r => r.RealmId.Id == realmId);
            if (realm == null) throw new InvalidOperationException("Realm doesn't exist. Id= " + realmId);

            return realm;
        }

        public Realm GetRealmByName(string realmId)
        {
            var id = new RealmId(realmId);
            if (realms.TryGetValue(id, out var realm)) return realm;

            throw new InvalidOperationException("Realm doesn't exist. Name=" + realmId);
        }

        public SchemaType GetSchemaType(SchemaTypeId schemaTypeId)
        {
            if (schemaTypes.TryGetValue(schemaTypeId, out var schemaType)) return schemaType;

            throw new InvalidOperationException("The type doesn't exist in the organization: " + schemaTypeId.FullId);
        }

        public Realm AddRealm(string realmId)
        {
            var rid = new RealmId(realmId);
            realms.Keys.EnsureIdDoesntExist(rid);
            return EventHandlerRegistry.Raise<AddedRealmToOrganizationEvent, Realm>(
                new AddedRealmToOrganizationEvent(OrganizationId, rid));
        }

        public SchemaType AddSchemaType(SchemaType schemaType)
        {
            if (schemaTypes.ContainsKey(schemaType.SchemaTypeId))
                throw new InvalidOperationException("Schema already exists");

            new SchemaTypeValidator(schemaType, schemaTypes.Values).Validate();
            var evt = new AddedSchemaTypeToOrganizationEvent(OrganizationId, schemaType);
            return EventHandlerRegistry.Raise<AddedSchemaTypeToOrganizationEvent, SchemaType>(evt);
        }
    }
}