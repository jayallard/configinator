using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Allard.Configinator.Infrastructure.MongoDb
{
    public class OrganizationRepositoryMongo : IOrganizationRepository, IOrganizationQueries
    {
        private const string Database = "configinator";
        private const string EventsCollectionName = "organization-events";
        private const string StateCollectionName = "organization-state";
        private readonly MongoClient client;

        static OrganizationRepositoryMongo()
        {
            BsonClassMap.RegisterClassMap<AddedConfigurationSectionToRealmEvent>();
            BsonClassMap.RegisterClassMap<AddedHabitatToRealmEvent>();
            BsonClassMap.RegisterClassMap<AddedRealmToOrganizationEvent>();
            BsonClassMap.RegisterClassMap<AddedSchemaTypeToOrganizationEvent>();
            BsonClassMap.RegisterClassMap<OrganizationCreatedEvent>();

            BsonClassMap.RegisterClassMap<SchemaTypeId>(cm =>
            {
                cm.MapProperty(st => st.FullId);
                cm.MapCreator(st => SchemaTypeId.Parse(st.FullId));
            });
        }

        public OrganizationRepositoryMongo()
        {
            client = new MongoClient("mongodb://localhost:27017");

            // unique index on state.OrganizationId
            var builder = Builders<OrganizationId>.IndexKeys;
            var indexModel = new CreateIndexModel<OrganizationId>(builder.Ascending(o => o.Id));
            GetStateCollection().Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<OrganizationId>> GetOrganizationIds()
        {
            // todo: hack
            return (await GetStateCollection().FindAsync(o => true)).ToList();
        }

        public async Task<OrganizationAggregate> GetOrganizationByIdAsync(string id)
        {
            var organization = (OrganizationAggregate) Activator.CreateInstance(typeof(OrganizationAggregate), true);
            var eventAccessor = new EventAccessor(organization);

            await GetEventSourceCollection()
                .Find(e => e.OrganizationId.Id == id)
                .Sort("{_id: 1}")
                .ForEachAsync(e => { eventAccessor.ApplyEvent(e.Event); });
            return organization;
        }

        public async Task UpdateAsync(OrganizationAggregate organization)
        {
            var events = GetEvents(organization);
            if (events.Count == 0) return;

            // todo: make sure it exists, but do that prior to calling...
            // this doesn't care

            // insert events
            await GetEventSourceCollection().InsertManyAsync(events);
            new EventAccessor(organization).ClearEvents();
        }

        private static List<EventDto> GetEvents(OrganizationAggregate organization)
        {
            var txId = Guid.NewGuid().ToString();
            var eventAccessor = new EventAccessor(organization);
            return eventAccessor
                .GetEvents()
                .Select(e =>
                    new EventDto(null, txId, e.EventId, organization.OrganizationId, e.EventDate, e.EventName, e))
                .ToList();
        }
        
        public async Task CreateAsync(OrganizationAggregate organization)
        {
            var events = GetEvents(organization);
            if (events.Count == 0) return;

            // note: transactions not supported with single instance...
            // this needs to be transaction.. .fix or switch to sql

            // create the state
            await GetStateCollection().InsertOneAsync(organization.OrganizationId);

            // insert events
            await GetEventSourceCollection().InsertManyAsync(events);
            new EventAccessor(organization).ClearEvents();
        }

        public async Task DevelopmentSetup()
        {
            await GetDatabase().DropCollectionAsync(StateCollectionName);
            await GetDatabase().DropCollectionAsync(EventsCollectionName);

            var kafkaType = SchemaTypeBuilder
                .Create("kafka/unsecured")
                .AddStringProperty("broker-list")
                .Build();

            var sqlType = SchemaTypeBuilder
                .Create("mssql/sql-user")
                .AddStringProperty("host")
                .AddStringProperty("user-id")
                .AddStringProperty("password", true)
                .AddStringProperty("instance", isOptional: true)
                .AddStringProperty("initial-catalog", isOptional: true)
                .Build();

            // var shovelServiceType = SchemaTypeBuilder
            //     .Create("something-domain/shovel-service")
            //     .AddProperty("sql-source", "mssql/sql-user")
            //     .AddProperty("kafka-target", "kafka/unsecured")
            //     .Build();

            var org = new OrganizationAggregate(new OrganizationId("allard"));
            org.AddSchemaType(kafkaType);
            org.AddSchemaType(sqlType);
            //org.AddSchemaType(shovelServiceType);

            var realm = org.AddRealm("domain-a");
            realm.AddHabitat("production", null);
            realm.AddHabitat("staging", null);
            realm.AddHabitat("dev", null);
            realm.AddHabitat("dev-allard", "dev");
            var properties = new List<SchemaTypeProperty>
            {
                new("sql-source", SchemaTypeId.Parse("mssql/sql-user"), false, true),
                new("kafka-target", SchemaTypeId.Parse("kafka/unsecured"), false, true),
            };
            realm.AddConfigurationSection("shovel-service", properties, "description");

            await CreateAsync(org);
        }

        private IMongoDatabase GetDatabase()
        {
            return client.GetDatabase(Database);
        }

        private IMongoCollection<EventDto> GetEventSourceCollection()
        {
            // i forget what should be cached or not... get everything
            // fresh until that's worked out.
            // todo: cache db? cache collection?
            return
                GetDatabase()
                    .GetCollection<EventDto>(EventsCollectionName);
        }

        private IMongoCollection<OrganizationId> GetStateCollection()
        {
            // i forget what should be cached or not... get everything
            // fresh until that's worked out.
            // todo: cache db? cache collection?
            return client
                .GetDatabase(Database)
                .GetCollection<OrganizationId>(StateCollectionName);
        }
    }
}