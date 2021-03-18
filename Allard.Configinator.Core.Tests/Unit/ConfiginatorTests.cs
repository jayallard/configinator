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

            var org = new OrganizationAggregate(new OrganizationId("allard"));
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
        public async Task SetFailsIfDocFailsValidation()
        {
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "staging");
            var setRequest =
                new SetConfigurationRequest(configId, ValueFormat.Raw,
                    JsonDocument.Parse("{ \"nothing-to\": \"see-here\" }"));
            var setResponse = await Configinator.SetValueAsync(setRequest);

            // there are 4 properties, and all 4 are null.
            // the passed in is is effectively ignored
            // because it doesn't have any of the properties
            // defined in the config section.
            setResponse.Failures.Count.Should().Be(4);
            setResponse.Success.Should().BeFalse();
        }

        [Fact]
        public async Task SetSucceedsIfDocPassesValidation()
        {
            var input = JsonDocument.Parse(TestUtility.GetFile("FullDocumentPasses.json"));
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "staging");
            var setRequest = new SetConfigurationRequest(configId, ValueFormat.Raw, input);
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Failures.Should().BeEmpty();
            setResponse.Success.Should().BeTrue();

            var getRequest = new GetValueRequest(configId, ValueFormat.Resolved);
            var get = await Configinator.GetValueAsync(getRequest);

            var expectedString = input.ToStupidComparisonString();
            var actualString = get.Value.ToStupidComparisonString();
            actualString.Should().Be(expectedString);
        }

        [Fact]
        public void JsonCompare()
        {
            var a = JsonDocument.Parse("{ \"a\": \"b\", \"c\": \"d\" }");
            var b = JsonDocument.Parse("   { \"c\": \"d\", \"a\": \"b\" }   ");
            testOutputHelper.WriteLine(a.Equals(b).ToString());
        }

        [Fact]
        public async Task OnlySubDocWillBeSaved()
        {
            // set everything at the base: dev   
            // set one value as a descendant: dev-jay
            // make sure only the one value is saved to dev-jay.
            var file = TestUtility.GetFile("FullDocumentPasses.json");
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "dev");
            var setRequest = new SetConfigurationRequest(configId, ValueFormat.Raw, JsonDocument.Parse(file));
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Success.Should().BeTrue();

            var configId2 = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "dev-allard");
            var sqlPassword = " { \"sql-source\": { \"password\": \"new password\" } } ";
            var setRequest2 =
                new SetConfigurationRequest(configId2, ValueFormat.Raw, JsonDocument.Parse(sqlPassword));
            await Configinator.SetValueAsync(setRequest2);
            var path = Organization
                .GetRealmByName(TestRealm1)
                .GetConfigurationSection(TestConfigurationSection1)
                .Path
                .Replace("{{habitat}}", "dev-allard");
            var value = (await ConfigStore.GetValueAsync(path)).Value.ConvertToString();
            value.Should().Be(JsonDocument.Parse(sqlPassword).ConvertToString());
        }
    }
}