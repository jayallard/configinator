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
        }

        public IEnumerable<OrganizationId> GetOrganizationIds()
        {
            // todo: hack
            return GetStateCollection().Find(o => true).ToList();
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

        public async Task SaveAsync(OrganizationAggregate organization)
        {
            var txId = Guid.NewGuid().ToString();
            var eventAccessor = new EventAccessor(organization);
            var events = eventAccessor
                .GetEvents()
                .Select(e =>
                    new EventDto(null, txId, e.EventId, organization.OrganizationId, e.EventDate, e.EventName, e))
                .ToList();
            if (events.Count == 0) return;

            // note: transactions not supported with single instance...
            // this needs to be transaction.. .fix or switch to sql

            // insert events
            await GetEventSourceCollection().InsertManyAsync(events);

            // update state
            var filter = Builders<OrganizationId>
                .Filter
                .Eq(o => o.Id, organization.OrganizationId.Id);
            await GetStateCollection().ReplaceOneAsync(filter, organization.OrganizationId, new ReplaceOptions
            {
                IsUpsert = true
            });

            eventAccessor.ClearEvents();
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
            realm.AddHabitat("production");
            realm.AddHabitat("staging");
            realm.AddHabitat("dev");
            realm.AddHabitat("dev-allard", "dev");
            var properties = new List<SchemaTypeProperty>
            {
                new("sql-source", SchemaTypeId.Parse("mssql/sql-user"), false, true),
                new("kafka-target", SchemaTypeId.Parse("kafka/unsecured"), false, true),
            };
            realm.AddConfigurationSection("shovel-service", properties,
                "/{{habitat}}/something-domain/shovel-service", "description");

            await SaveAsync(org);
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