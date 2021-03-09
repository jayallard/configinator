using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Allard.Configinator.Infrastructure.MongoDb
{
    public class OrganizationRepositoryMongo : IOrganizationRepository, IOrganizationQueries
    {
        private const string Database = "configinator";
        private const string Collection = "organization-events";
        private readonly MongoClient client;

        static OrganizationRepositoryMongo()
        {
            //todo: need a better way to do this
            BsonClassMap.RegisterClassMap<AddedConfigurationSectionToRealmEvent>();
            BsonClassMap.RegisterClassMap<AddedHabitatToRealmEvent>();
            BsonClassMap.RegisterClassMap<AddedRealmToOrganizationEvent>();
            BsonClassMap.RegisterClassMap<AddedSchemaTypeToOrganizationEvent>();
            BsonClassMap.RegisterClassMap<OrganizationCreatedEvent>();
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
                .Find(e => e.OrganizationId == id)
                .Sort("{_id: 1}")
                .ForEachAsync(e => { eventAccessor.ApplyEvent(e.Event); });
            return organization;
        }

        public Task<OrganizationAggregate> GetOrganizationByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task SaveAsync(OrganizationAggregate organization)
        {
            var txId = Guid.NewGuid().ToString();
            var eventAccessor = new EventAccessor(organization);
            var events = eventAccessor
                .GetEvents()
                .Select(e =>
                    new EventDto(null, txId, e.EventId, organization.OrganizationId.Id, e.EventDate, e.EventName, e))
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

        private IMongoCollection<EventDto> GetEventSourceCollection()
        {
            // i forget what should be cached or not... get everything
            // fresh until that's worked out.
            // todo: cache db? cache collection?
            return client
                .GetDatabase(Database)
                .GetCollection<EventDto>("organization-events");
        }

        private IMongoCollection<OrganizationId> GetStateCollection()
        {
            // i forget what should be cached or not... get everything
            // fresh until that's worked out.
            // todo: cache db? cache collection?
            return client
                .GetDatabase(Database)
                .GetCollection<OrganizationId>("organization-state");
        }
    }
}