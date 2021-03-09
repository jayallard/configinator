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

        private IConfigStore ConfigStore { get; }
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

            var shovelServiceType = SchemaTypeBuilder
                .Create("something-domain/shovel-service")
                .AddProperty("sql-source", "mssql/sql-user")
                .AddProperty("kafka-target", "kafka/unsecured")
                .Build();

            var org = new OrganizationAggregate(OrganizationId.NewOrganizationId("allard"));
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

        [Fact]
        public async Task GetValueDoesntExist()
        {
            var configId = new ConfigurationId(Organization.OrganizationId.Name, "staging", TestRealm1,
                TestConfigurationSection1);
            var request = new GetConfigurationRequest(configId);
            var value = await Configinator.GetValueAsync(request);
            value.Existing.Should().BeFalse();
            value.ConfigurationId.Should().Be(configId);
            value.PropertyDetail.Count.Should().Be(0);
        }

        [Fact]
        public async Task SetFailsIfDocFailsValidation()
        {
            var configId = new ConfigurationId(Organization.OrganizationId.Name, "staging", TestRealm1,
                TestConfigurationSection1);
            var setRequest =
                new SetConfigurationRequest(configId, "not in use yet", "{ \"nothing-to\": \"see-here\" }");
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Failures.Count.Should().Be(2);
            setResponse.Success.Should().BeFalse();
        }

        [Fact]
        public async Task SetSucceedsIfDocPassesValidation()
        {
            var file = TestUtility.GetFile("FullDocumentPasses.json");
            var configId = new ConfigurationId(Organization.OrganizationId.Name, "staging", TestRealm1,
                TestConfigurationSection1);
            var setRequest = new SetConfigurationRequest(configId, "not in use yet", file);
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Failures.Should().BeEmpty();
            setResponse.Success.Should().BeTrue();


            var getRequest = new GetConfigurationRequest(configId);
            var get = await Configinator.GetValueAsync(getRequest);

            // todo: broken/fragile - deep compare doesnt exist
            //file.Should().Be(get.ResolvedValue);
        }

        [Fact]
        public async Task OnlySubDocWillBeSaved()
        {
            // set everything at the base: dev   
            // set one value as a descendant: dev-jay
            // make sure only the one value is saved to dev-jay.
            var file = TestUtility.GetFile("FullDocumentPasses.json");
            var configId = new ConfigurationId(Organization.OrganizationId.Name, "dev", TestRealm1,
                TestConfigurationSection1);
            var setRequest = new SetConfigurationRequest(configId, "not in use yet", file);
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Success.Should().BeTrue();

            var configId2 = new ConfigurationId(Organization.OrganizationId.Name, "dev-allard", TestRealm1,
                TestConfigurationSection1);
            var sqlPassword = " { \"sql-source\": { \"password\": \"new password\" } } ";
            var setRequest2 = new SetConfigurationRequest(configId2, "not in use yet", sqlPassword);
            var setResponse2 = await Configinator.SetValueAsync(setRequest2);
            var path = Organization
                .GetRealmByName(TestRealm1)
                .GetConfigurationSection(TestConfigurationSection1)
                .Path
                .Replace("{{habitat}}", "dev-allard");
            var value = await ConfigStore.GetValueAsync(path);
            value.Value.Should().Be(sqlPassword);
            testOutputHelper.WriteLine(value.Value);
        }
    }
}