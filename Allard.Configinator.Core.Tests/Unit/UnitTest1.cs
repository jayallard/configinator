using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Yadda()
        {
            var file = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "__TestFiles",
                "Test1.yaml");

            var doc = File.ReadAllText(file);

            var deser = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            var org = deser.Deserialize<OrganizationDto>(doc);
            Console.Write("");
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
                .Register<SomethingEvent>(_ => r1 = "boo yea")
                .Register<SomethingElseEvent>(_ => r2 = "santa claus")
                .Build();

            registry.Raise(new SomethingEvent());
            registry.Raise(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        [Fact(Skip = "Demonstrating a known issue.")]
        public void IfTypesAreWrong()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(_ => string.Empty)
                .Build();
            registry.Raise<SomethingEvent, int>(new SomethingEvent());
        }

        [Fact]
        public void EvenHandlerFunction()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(_ => "boo yea")
                .Register<SomethingElseEvent, string>(_ => "santa claus")
                .Build();

            var r1 = registry.Raise<SomethingEvent, string>(new SomethingEvent());
            var r2 = registry.Raise<SomethingElseEvent, string>(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        [Fact]
        public void AddConfigurationSectionFailsIfSchemaTypeDoesntExist()
        {
            var orgId = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(orgId);
            var realm = org.AddRealm("blah");
            Action test = () => realm.AddConfigurationSection("cs", SchemaTypeId.Parse("a/b"), "path", "description");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("The type doesn't exist in the organization: a/b");
        }

        [Fact]
        public void AddDuplicateRealmFails()
        {
            var orgId = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(orgId);

            org.Realms.Should().BeEmpty();
            org.AddRealm("ALLARD-REALM-1");
            Action test = () => org.AddRealm("ALLARD-REALM-1");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("A RealmId with that name already exists. Name=allard-realm-1");
        }

        [Fact]
        public void Build()
        {
            var orgId = OrganizationId.NewOrganizationId();
            var org = new OrganizationAggregate(orgId);

            org.Realms.Should().BeEmpty();
            var realm = org.AddRealm("ALLARD-REALM-1");
            org.Realms.Single().Should().Be(realm);
            realm.RealmId.Name.Should().Be("allard-realm-1");

            realm.Habitats.Should().BeEmpty();
            var habitat = realm.AddHabitat("Production");
            realm.Habitats.Single().Should().Be(habitat);
            habitat.HabitatId.Name.Should().Be("production");

            var schemaTypeId = SchemaTypeId.Parse("a/b");
            var schemaType = new SchemaType(
                schemaTypeId,
                new List<Property>().AsReadOnly(),
                new List<PropertyGroup>().AsReadOnly());
            org.SchemaTypes.Should().BeEmpty();
            var addedType = org.AddSchemaType(schemaType);
            addedType.Should().Be(schemaType);
            org.SchemaTypes.Count.Should().Be(1);

            realm.ConfigurationSections.Should().BeEmpty();
            var cs = realm.AddConfigurationSection("Test1", schemaTypeId, "/a/b/c",
                "description");
            realm.ConfigurationSections.Single().Should().Be(cs);
            cs.ConfigurationSectionId.Name.Should().Be("test1");
        }

        [Fact]
        public void SerializationTest()
        {
            var organizationId = new OrganizationId(Guid.NewGuid().ToString());
            var evt = new AddedRealmToOrganizationEvent(organizationId, new RealmId("id", "name"));
            var json = JsonSerializer.Serialize(evt);

            JsonSerializer.Deserialize(json, typeof(AddedRealmToOrganizationEvent));
            testOutputHelper.WriteLine(json);
        }

        public record SomethingEvent : DomainEvent;

        public record SomethingElseEvent : DomainEvent;
    }
}