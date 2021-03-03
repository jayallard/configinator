using System;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Infrastructure.MongoDb.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test1()
        {
            var orgId = Guid.NewGuid().ToString();
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("test1");
            db.DropCollection("organization-events");
            var collection = db.GetCollection<EventDto>("organization-events");

            var evt = new RealmCreatedEvent(new OrganizationId(orgId), new RealmId("realm id", "realm name"));
            var dto = new EventDto(null, Guid.NewGuid().ToString(),Guid.NewGuid().ToString(), orgId, DateTime.UtcNow, "BlahBlah", evt);
            collection.InsertOne(dto);

            var find = collection.FindSync(d => d.OrganizationId == orgId).Single();
            find.Should().NotBeNull();
            find.Event.Should().BeOfType<RealmCreatedEvent>();
        }

        [Fact]
        public async Task BigAggregate()
        {
            // create
            var repo = new OrganizationRepositoryMongo();
            var orgId = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(orgId);
            for (var i = 0; i < 500; i++)
            {
                var realm = org.CreateRealm("realm " + i);
                for (var i2 = 0; i2 < 20; i2++)
                {
                    realm.CreateHabitat("h " + i2);
                }
            }

            await repo.SaveAsync(org);

            var read = await repo.GetOrganizationAsync(orgId.Id);
            read.Realms.Count.Should().Be(500);
        }

        [Fact]
        public async Task WriteGet()
        {
            // create
            var repo = new OrganizationRepositoryMongo();
            var orgId = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(orgId);
            var r1 = org.CreateRealm("realm a");
            r1.CreateHabitat("a");
            r1.CreateHabitat("b");

            var r2 = org.CreateRealm("realm b");
            r2.CreateHabitat("x");
            r2.CreateHabitat("y");
            r2.CreateHabitat("z");

            await repo.SaveAsync(org);
            // read
            var read = await repo.GetOrganizationAsync(orgId.Id);
            read.Realms.Count.Should().Be(2);
            var readRealm1 = read.Realms.Single(r => r.Id == r1.Id);
            readRealm1.Habitats.Count.Should().Be(2);
            var readRealm2 = read.Realms.Single(r => r.Id == r2.Id);
            readRealm2.Habitats.Count.Should().Be(3);

            // update
            read.CreateRealm("yay!");
            await repo.SaveAsync(read);

            var readAfterUpdate = await repo.GetOrganizationAsync(orgId.Id);
            readAfterUpdate.Realms.Count.Should().Be(3);
        }
    }
}