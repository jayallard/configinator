using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using Allard.Configinator.Core.ObjectVersioning;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class ConfiginatorTests
    {
        private const string OrganizationId = "allard";
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

            var org = new OrganizationAggregate(new OrganizationId(OrganizationId));
            org.AddSchemaType(kafkaType);
            org.AddSchemaType(sqlType);

            var realm = org.AddRealm(TestRealm1);
            realm.AddHabitat("production", null);
            realm.AddHabitat("staging", null);
            realm.AddHabitat("dev", null);
            realm.AddHabitat("dev-allard", "dev");
            realm.AddHabitat("dev-allard2", "dev-allard");

            var properties = new List<SchemaTypeProperty>
            {
                new("sql-source", SchemaTypeId.Parse("mssql/sql-user"), false, true),
                new("kafka-target", SchemaTypeId.Parse("kafka/unsecured"), false, true)
            };

            realm.AddConfigurationSection(TestConfigurationSection1, properties, "description");
            return org;
        }

        private static ConfigurationId CreateConfigurationId(string habitatId)
        {
            return new(OrganizationId, TestRealm1, TestConfigurationSection1, habitatId);
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

        /// <summary>
        ///     Given: dev = x, and dev-allard=x, and dev-allard2=x.
        ///     When: dev is changed to y
        ///     Then: dev-allard is changed to y, and dev-allard2 is changed to y.
        ///     --
        ///     Follow-up:
        ///     When: dev-allard is changed to z
        ///     Then: dev is not changed. dev-allard2 is changed to z.
        /// </summary>
        [Fact]
        public async Task InheritedValueIsUpdatedWhenParentChanges()
        {
            var input = JsonDocument
                .Parse(TestUtility.GetFile("FullDocumentPasses.json"))
                .ToObjectDto();

            var idDev = CreateConfigurationId("dev");
            var idDevAllard = CreateConfigurationId("dev-allard");
            var idDevAllard2 = CreateConfigurationId("dev-allard2");
            // ----------------------------------------------------------------
            //   setup and setup confirmation
            // ----------------------------------------------------------------
            // set the value for DEV, and it will cascade down.
            var setup = input
                .Clone()
                .SetValue("sql-source/host", "FirstValue")
                .ToJson();

            var setupResult = await Configinator.SetValueAsync(new SetValueRequest(idDev, null, setup));
            setupResult.Habitats.Count.Should().Be(3);
            setupResult.Habitats.All(h => h.Saved).Should().BeTrue();

            // make sure it cascaded before changing values.
            var setupConfirmation = await Configinator.GetValueDetailAsync(new GetValueRequest(idDevAllard2));
            setupConfirmation.Habitats.Count.Should().Be(3);
            setupConfirmation
                .Value
                .Objects.Single(o => o.Name == "sql-source")
                .Properties.Single(p => p.Name == "host")
                .HabitatValues.All(v => v.Value == "FirstValue")
                .Should().BeTrue();

            // ----------------------------------------------------------------
            //   update DEV and confirm it cascades down to
            //   DEV-ALLARD and DEV-ALLARD2
            // ----------------------------------------------------------------
            // change the value of the top layer: dev.
            // dev-allard and dev-allard2 will inherit.
            var setDev = input
                .Clone()
                .SetValue("sql-source/host", "SecondValue")
                .ToJson();
            var setDevResult = await Configinator.SetValueAsync(new SetValueRequest(idDev, null, setDev));
            setDevResult.Habitats.Count.Should().Be(3);
            setDevResult.Habitats.All(h => h.Saved).Should().BeTrue();

            // confirm the new value cascaded down
            var devConfirmation = await Configinator.GetValueDetailAsync(new GetValueRequest(idDevAllard2));
            devConfirmation.Habitats.Count.Should().Be(3);
            devConfirmation
                .Value
                .Objects.Single(o => o.Name == "sql-source")
                .Properties.Single(p => p.Name == "host")
                .HabitatValues.All(v => v.Value == "SecondValue")
                .Should().BeTrue();

            // ----------------------------------------------------------------
            //   update DEV-ALLARD and confirm it cascades down to
            //   DEV-ALLARD2. DEV doesn't change.
            // ----------------------------------------------------------------
            // change the value of the middle layer: dev-allard
            // dev-allard2 will inherit. dev will not change.
            var setAllard = input
                .Clone()
                .SetValue("sql-source/host", "ThirdValue")
                .ToJson();
            var setAllardResult = await Configinator.SetValueAsync(new SetValueRequest(idDevAllard, null, setAllard));
            setAllardResult.Habitats.Count.Should().Be(2);
            setAllardResult.Habitats.All(h => h.Saved).Should().BeTrue();

            var allardConfirmation = await Configinator.GetValueDetailAsync(new GetValueRequest(idDevAllard2));
            var allardHostProperty = allardConfirmation
                .Value
                .Objects.Single(o => o.Name == "sql-source")
                .Properties.Single(p => p.Name == "host");
            allardHostProperty.HabitatValues.Single(h => h.HabitatId == "dev").Value.Should().Be("SecondValue");
            allardHostProperty.HabitatValues.Single(h => h.HabitatId == "dev-allard").Value.Should().Be("ThirdValue");
            allardHostProperty.HabitatValues.Single(h => h.HabitatId == "dev-allard2").Value.Should().Be("ThirdValue");
        }

        [Fact]
        public async Task DetailedValue()
        {
            var input = JsonDocument
                .Parse(TestUtility.GetFile("FullDocumentPasses.json"))
                .ToObjectDto();

            var id1 = CreateConfigurationId("dev");
            var set1 = input
                .Clone()
                .SetValue("sql-source/host", "dev")
                .ToJson();
            var set1Result = await Configinator.SetValueAsync(new SetValueRequest(id1, null, set1));
            set1Result.Habitats.Count.Should().Be(3);
            set1Result.Habitats.All(h => h.Saved).Should().BeTrue();

            var id2 = CreateConfigurationId("dev-allard");
            var set2 = input
                .Clone()
                .SetValue("sql-source/host", "dev-allard")
                .ToJson();
            var set2Result = await Configinator.SetValueAsync(new SetValueRequest(id2, null, set2));
            set2Result.Habitats.Count.Should().Be(2);
            set2Result.Habitats.All(h => h.Saved).Should().BeTrue();

            var id3 = CreateConfigurationId("dev-allard2");
            var set3 = input
                .Clone()
                .SetValue("sql-source/host", "dev-allard2")
                .ToJson();
            var set3Result = await Configinator.SetValueAsync(new SetValueRequest(id3, null, set3));
            set3Result.Habitats.Count.Should().Be(1);
            set3Result.Habitats.All(h => h.Saved).Should().BeTrue();


            // the hierarchy is (from top) dev/dev-allard/dev-allard2.
            // each GET will include the requested habitat, and its bases.
            // dev returns 1, dev-allard returns 2, dev-allard2 returns 3.
            {
                var get1 = new GetValueRequest(id1);
                var get1Result = await Configinator.GetValueDetailAsync(get1);
                get1Result.Habitats.Count.Should().Be(1);
                var host1 = get1Result.Value.Objects.Single(o => o.Name == "sql-source").Properties
                    .Single(p => p.Name == "host");
                host1.HabitatValues.Count.Should().Be(1);
                host1.ResolvedValue.Should().Be("dev");
                host1.HabitatValues[0].Value.Should().Be("dev");
            }

            {
                var get2 = new GetValueRequest(id2);
                var get2Result = await Configinator.GetValueDetailAsync(get2);
                get2Result.Habitats.Count.Should().Be(2);
                var host2 = get2Result.Value.Objects.Single(o => o.Name == "sql-source").Properties
                    .Single(p => p.Name == "host");
                host2.HabitatValues.Count.Should().Be(2);
                host2.ResolvedValue.Should().Be("dev-allard");
                host2.HabitatValues[0].Value.Should().Be("dev");
                host2.HabitatValues[1].Value.Should().Be("dev-allard");
            }

            {
                var get3 = new GetValueRequest(id3);
                var get3Result = await Configinator.GetValueDetailAsync(get3);
                get3Result.Habitats.Count.Should().Be(3);
                var host3 = get3Result.Value.Objects.Single(o => o.Name == "sql-source").Properties
                    .Single(p => p.Name == "host");
                host3.HabitatValues.Count.Should().Be(3);
                host3.ResolvedValue.Should().Be("dev-allard2");
                host3.HabitatValues[0].Value.Should().Be("dev");
                host3.HabitatValues[1].Value.Should().Be("dev-allard");
                host3.HabitatValues[2].Value.Should().Be("dev-allard2");
            }
        }
    }
}