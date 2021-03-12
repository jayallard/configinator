using System;
using System.Linq;
using System.Threading.Tasks;
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
            var orgId = OrganizationId.NewOrganizationId("Allard");
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("test1");
            db.DropCollection("organization-events");
            var collection = db.GetCollection<EventDto>("organization-events");

            var evt = new AddedRealmToOrganizationEvent(orgId,
                new RealmId("realm-id"));
            var dto = new EventDto(null, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), orgId,
                DateTime.UtcNow,
                "BlahBlah", evt);
            collection.InsertOne(dto);

            var find = collection.FindSync(d => d.OrganizationId == orgId).Single();
            find.Should().NotBeNull();
            find.Event.Should().BeOfType<AddedRealmToOrganizationEvent>();
        }

        [Fact]
        public async Task ResetAndRead()
        {
            var repo = new OrganizationRepositoryMongo();
            await repo.DevelopmentSetup();
            var org = repo.GetOrganizationByNameAsync("allard");
        }

        [Fact]
        public async Task BigAggregate()
        {
            const int realmCount = 10;
            const int habitCountPerRealm = 10;
            // create
            var repo = new OrganizationRepositoryMongo();
            var orgId = OrganizationId.NewOrganizationId("Allard");
            var org = new OrganizationAggregate(orgId);
            for (var i = 0; i < realmCount; i++)
            {
                var realm = org.AddRealm("realm " + i);
                for (var i2 = 0; i2 < habitCountPerRealm; i2++) realm.AddHabitat("h " + i2);
            }

            await repo.SaveAsync(org);

            var read = await repo.GetOrganizationByIdAsync(orgId.Id);
            read.Realms.Count.Should().Be(realmCount);
        }

        [Fact]
        public async Task WriteGet()
        {
            // create
            var repo = new OrganizationRepositoryMongo();
            var orgId = OrganizationId.NewOrganizationId("Allard");
            var org = new OrganizationAggregate(orgId);
            var r1 = org.AddRealm("realm a");
            r1.AddHabitat("a");
            r1.AddHabitat("b");

            var r2 = org.AddRealm("realm b");
            r2.AddHabitat("x");
            r2.AddHabitat("y");
            r2.AddHabitat("z");

            await repo.SaveAsync(org);
            // read
            var read = await repo.GetOrganizationByIdAsync(orgId.Id);
            read.Realms.Count.Should().Be(2);
            var readRealm1 = read.Realms.Single(r => r.RealmId == r1.RealmId);
            readRealm1.Habitats.Count.Should().Be(2);
            var readRealm2 = read.Realms.Single(r => r.RealmId == r2.RealmId);
            readRealm2.Habitats.Count.Should().Be(3);

            // update
            read.AddRealm("yay!");
            await repo.SaveAsync(read);

            var readAfterUpdate = await repo.GetOrganizationByIdAsync(orgId.Id);
            readAfterUpdate.Realms.Count.Should().Be(3);
        }
    }
}