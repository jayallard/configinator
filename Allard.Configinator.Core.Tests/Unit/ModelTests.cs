using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class ModelTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ModelTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void AddConfigurationSectionFailsIfSchemaTypeDoesntExist()
        {
            var orgId = new OrganizationId("allard");
            var org = new OrganizationAggregate(orgId);
            var realm = org.AddRealm("blah");
            var properties = new List<SchemaTypeProperty>
            {
                new("name", SchemaTypeId.Parse("a/b"))
            };
            Action test = () => realm.AddConfigurationSection("cs", properties, "path", "description");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("The type doesn't exist in the organization: a/b");
        }

        [Fact]
        public void AddDuplicateRealmFails()
        {
            var orgId = new OrganizationId("allard");
            var org = new OrganizationAggregate(orgId);

            org.Realms.Should().BeEmpty();
            org.AddRealm("ALLARD-REALM-1");
            Action test = () => org.AddRealm("ALLARD-REALM-1");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("RealmId already exists. Id=allard-realm-1");
        }

        [Fact]
        public void Build()
        {
            var orgId = new OrganizationId("allard");
            var org = new OrganizationAggregate(orgId);

            org.Realms.Should().BeEmpty();
            var realm = org.AddRealm("ALLARD-REALM-1");
            org.Realms.Single().Should().Be(realm);
            realm.RealmId.Id.Should().Be("allard-realm-1");

            realm.Habitats.Should().BeEmpty();
            var habitat = realm.AddHabitat("Production");
            realm.Habitats.Single().Should().Be(habitat);
            habitat.HabitatId.Id.Should().Be("production");

            var schemaType = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("blah", SchemaTypeId.String)
                .Build();
            org.SchemaTypes.Should().BeEmpty();
            var addedType = org.AddSchemaType(schemaType);
            addedType.Should().Be(schemaType);
            org.SchemaTypes.Count.Should().Be(1);

            realm.ConfigurationSections.Should().BeEmpty();
            var properties = new List<SchemaTypeProperty>
            {
                new("boo", SchemaTypeId.Parse("a/b"))
            };
            var cs = realm.AddConfigurationSection("Test1", properties, "/a/b/c",
                "description");
            realm.ConfigurationSections.Single().Should().Be(cs);
            cs.SectionId.Id.Should().Be("test1");
        }

        [Fact]
        public void SerializationTest()
        {
            var organizationId = new OrganizationId("allard");
            var evt = new AddedRealmToOrganizationEvent(organizationId, new RealmId("id"));
            var json = JsonSerializer.Serialize(evt);

            JsonSerializer.Deserialize(json, typeof(AddedRealmToOrganizationEvent));
            testOutputHelper.WriteLine(json);
        }
    }
}