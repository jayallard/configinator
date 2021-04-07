using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class ConfiginatorTests
    {
        private const string TestRealm1 = "something-domain";
        private const string TestConfigurationSection1 = "shovel-service";
        private readonly ITestOutputHelper testOutputHelper;

        public ConfiginatorTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            Organization = CreateOrganization();
            ConfigStore = new MemoryConfigStore();
            Configinator = new Configinator(Organization, ConfigStore);
        }

        private MemoryConfigStore ConfigStore { get; }
        private Configinator Configinator { get; }
        private OrganizationAggregate Organization { get; }

        private static OrganizationAggregate CreateOrganization()
        {
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

            var org = new OrganizationAggregate(new OrganizationId("allard"));
            org.AddSchemaType(kafkaType);
            org.AddSchemaType(sqlType);
            //org.AddSchemaType(shovelServiceType);

            var realm = org.AddRealm(TestRealm1);
            realm.AddHabitat("production", null);
            realm.AddHabitat("staging", null);
            realm.AddHabitat("dev", null);
            realm.AddHabitat("dev-allard", "dev");

            var properties = new List<SchemaTypeProperty>
            {
                new("sql-source", SchemaTypeId.Parse("mssql/sql-user"), false, true),
                new("kafka-target", SchemaTypeId.Parse("kafka/unsecured"), false, true)
            };

            realm.AddConfigurationSection(TestConfigurationSection1, properties, "description");
            return org;
        }

        [Fact]
        public async Task SetFailsIfDocFailsValidation()
        {
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "staging");
            var setRequest =
                new SetValueRequest(configId, null,
                    JsonDocument.Parse("{ \"nothing-to\": \"see-here\" }"));
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Habitats.Single().ValidationFailures.Count.Should().Be(4);

            // it's not changed because no matching properties.
            // nothing was touched.
            setResponse.Habitats.Single().Changed.Should().BeFalse();
            setResponse.Habitats.Single().Saved.Should().BeFalse();
        }

        [Fact]
        public async Task SetSucceedsIfDocPassesValidation()
        {
            var input = JsonDocument.Parse(TestUtility.GetFile("FullDocumentPasses.json"));
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "staging");
            var setRequest = new SetValueRequest(configId, null, input);
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Habitats.Count.Should().Be(1);
            setResponse.Habitats.Single().ValidationFailures.Should().BeEmpty();
            setResponse.Habitats.Single().Saved.Should().BeTrue();
            setResponse.Habitats.Single().Changed.Should().BeTrue();

            ConfigStore.Values.Count.Should().Be(1);
            var value = ConfigStore.Values.Values.Single().Value.RootElement;
            var sql = value.GetProperty("sql-source");
            sql.EnumerateObject().ToList().Count.Should().Be(5);
            sql.GetProperty("password").GetString().Should().Be("password");
            sql.GetProperty("host").GetString().Should().Be("localhost");
            sql.GetProperty("user-id").GetString().Should().Be("sa");
            sql.GetProperty("instance").GetString().Should().BeNull();
            sql.GetProperty("initial-catalog").GetString().Should().BeNull();

            value.GetProperty("kafka-target").EnumerateObject().Count().Should().Be(1);
            value.GetProperty("kafka-target").GetProperty("broker-list").GetString().Should().Be("localhost:9092");

            var request = new GetValueRequest(configId, false);
            var x = await Configinator.GetValueDetailAsync(request);
            testOutputHelper.WriteLine("");
        }

        [Fact]
        public async Task SetPropertyByPath()
        {
            var input = JsonDocument.Parse(TestUtility.GetFile("FullDocumentPasses.json"));
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "staging");
            var setRequest = new SetValueRequest(configId, null, input);
            await Configinator.SetValueAsync(setRequest);

            var setProperty = new SetValueRequest(configId, "/sql-source/user-id", JsonDocument.Parse("\"partial\""));
            await Configinator.SetValueAsync(setProperty);
            ConfigStore.Values.Count.Should().Be(1);
            ConfigStore.Values.Values.Single().Value.RootElement.GetProperty("sql-source").GetProperty("user-id")
                .GetString().Should().Be("partial");
        }

        [Fact]
        public async Task WhenValueDoesntExistReturnsStructure()
        {
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "staging");
            var get = new GetValueRequest(configId);
            var response = await Configinator.GetValueAsync(get);
            response.Exists.Should().Be(false);
            testOutputHelper.WriteLine(response.Value.RootElement.ToString());
        }
    }
}