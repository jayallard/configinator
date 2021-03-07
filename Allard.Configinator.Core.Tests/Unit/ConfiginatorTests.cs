using System;
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
        private readonly ITestOutputHelper testOutputHelper;

        public ConfiginatorTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        private const string TestRealm1 = "something-domain";
        private const string TestConfigurationSection1 = "shovel-service";

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

            var shovelServiceType = SchemaTypeBuilder
                .Create("something-domain/shovel-service")
                .AddProperty("sql-source", "mssql/sql-user")
                .AddProperty("kafka-target", "kafka/unsecured")
                .Build();

            var org = new OrganizationAggregate(OrganizationId.NewOrganizationId());
            org.AddSchemaType(kafkaType);
            org.AddSchemaType(sqlType);
            org.AddSchemaType(shovelServiceType);

            var realm = org.AddRealm(TestRealm1);
            realm.AddHabitat("production");
            realm.AddHabitat("staging");
            realm.AddHabitat("dev");
            realm.AddHabitat("dev-allard", "dev");
            realm.AddConfigurationSection(TestConfigurationSection1, "something-domain/shovel-service",
                "/{{habitat}}/something-domain/shovel-service", "description");
            return org;
        }

        private static Configinator GetConfiginator() =>
            new(CreateOrganization(), new MemoryConfigStore());


        [Fact]
        public async Task GetValueDoesntExist()
        {
            var configinator = GetConfiginator();
            var configId = new ConfigurationId("staging", TestRealm1, TestConfigurationSection1);
            var request = new GetConfigurationRequest(configId);
            var value = await configinator.GetValueAsync(request);
            value.Existing.Should().BeFalse();
            value.ConfigurationId.Should().Be(configId);
            value.PropertyDetail.Count.Should().Be(0);
        }

        [Fact]
        public async Task SetFailsIfDocFailsValidation()
        {
            var configinator = GetConfiginator();
            var configId = new ConfigurationId("staging", TestRealm1, TestConfigurationSection1);
            var setRequest =
                new SetConfigurationRequest(configId, "not in use yet", "{ \"nothing-to\": \"see-here\" }");
            var setResponse = await configinator.SetValueAsync(setRequest);
            setResponse.Failures.Count.Should().Be(2);
            setResponse.Success.Should().BeFalse();
        }

        [Fact]
        public async Task SetSucceedsIfDocPassesValidation()
        {
            var configinator = GetConfiginator();
            var file = TestUtility.GetFile("FullDocumentPasses.json");
            var configId = new ConfigurationId("staging", TestRealm1, TestConfigurationSection1);
            var setRequest = new SetConfigurationRequest(configId, "not in use yet", file);
            var setResponse = await configinator.SetValueAsync(setRequest);
            setResponse.Failures.Should().BeEmpty();
            setResponse.Success.Should().BeTrue();
            

            var getRequest = new GetConfigurationRequest(configId);
            var get = await configinator.GetValueAsync(getRequest);
            
            // todo: broken/fragile - deep compare doesnt exist
            //file.Should().Be(get.ResolvedValue);
        }
        
        // todo: test a hierarchy. make sure only the subdoc is saved 
    }
}