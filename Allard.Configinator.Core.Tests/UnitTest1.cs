using System;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Schema;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests
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
            var id = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(id);
            org.OrganizationId.Should().Be(id);
        }

        [Fact]
        public void EventHandlerAction()
        {
            var r1 = string.Empty;
            var r2 = string.Empty;
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent>(e => r1 = "boo yea")
                .Register<SomethingElseEvent>(e => r2 = "santa claus")
                .Build();

            registry.Raise(new SomethingEvent());
            registry.Raise(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        [Fact]
        public void IfTypesAreWrong()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(evt => string.Empty)
                .Build();
            var x = registry.Raise<SomethingEvent, int>(new SomethingEvent());
        }

        [Fact]
        public void EvenHandlerFunction()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(e => "boo yea")
                .Register<SomethingElseEvent, string>(e => "santa claus")
                .Build();

            var r1 = registry.Raise<SomethingEvent, string>(new SomethingEvent());
            var r2 = (string) registry.Raise<SomethingElseEvent, string>(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        [Fact]
        public void Build()
        {
            var orgId = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(orgId);

            org.Realms.Should().BeEmpty();
            var realm = org.CreateRealm("Allard-Realm-1");
            org.Realms.Single().Should().Be(realm);
            realm.Id.Name.Should().Be("Allard-Realm-1");

            realm.Habitats.Should().BeEmpty();
            var habitat = realm.CreateHabitat("Production");
            realm.Habitats.Single().Should().Be(habitat);
            habitat.Id.Name.Should().Be("Production");
        }

        [Fact]
        public void SerializationTest()
        {
            var organizationId = new OrganizationId(Guid.NewGuid().ToString());
            var evt = new RealmCreatedEvent(organizationId, new RealmId("id", "name"));
            var json = JsonSerializer.Serialize(evt);

            var blah = JsonSerializer.Deserialize(json, typeof(RealmCreatedEvent));
            testOutputHelper.WriteLine(json);
        }
        
        public record SomethingEvent : DomainEvent;

        public record SomethingElseEvent : DomainEvent;
    }
}