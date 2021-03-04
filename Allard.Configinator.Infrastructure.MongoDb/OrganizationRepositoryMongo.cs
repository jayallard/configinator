using System;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using MongoDB.Driver;

namespace Allard.Configinator.Infrastructure.MongoDb
{
    public class OrganizationRepositoryMongo : IOrganizationRepository
    {
        private const string Database = "configinator";
        private const string Collection = "organization-events";
        private readonly MongoClient client;

        public OrganizationRepositoryMongo()
        {
            client = new MongoClient("mongodb://localhost:27017");
            client.GetDatabase(Database).DropCollection(Collection);
        }

        public async Task<OrganizationAggregate> GetOrganizationAsync(string id)
        {
            var organization = (OrganizationAggregate) Activator.CreateInstance(typeof(OrganizationAggregate), true);
            var eventAccessor = new EventAccessor(organization);

            await GetCollection()
                .Find(e => e.OrganizationId == id)
                //.Sort("{_id: 1")
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
                    new EventDto(null, txId, e.EventId, organization.OrganizationId.Id, e.EventDate, e.EventName, e))
                .ToList();
            if (events.Count == 0) return;

            await GetCollection()
                .InsertManyAsync(events);
            eventAccessor.ClearEvents();
        }

        private IMongoCollection<EventDto> GetCollection()
        {
            // i forget what should be cached or not... get everything
            // fresh until that's worked out.
            // todo: cache db? cache collection?
            return client
                .GetDatabase(Database)
                .GetCollection<EventDto>("organization-events");
        }
    }
}