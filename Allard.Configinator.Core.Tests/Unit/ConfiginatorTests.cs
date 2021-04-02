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

            // var shovelServiceType = SchemaTypeBuilder
            //     .Create("something-domain/shovel-service")
            //     .AddProperty("sql-source", "mssql/sql-user")
            //     .AddProperty("kafka-target", "kafka/unsecured")
            //     .Build();

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
            var setRequest = new SetValueRequest(configId, null, input);
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Failures.Should().BeEmpty();
            setResponse.Success.Should().BeTrue();

            var getRequest = new GetValueRequest(configId);
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
            var setRequest = new SetValueRequest(configId, null, JsonDocument.Parse(file));
            var setResponse = await Configinator.SetValueAsync(setRequest);
            setResponse.Success.Should().BeTrue();

            var configId2 = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "dev-allard");
            var sqlPassword = " { \"sql-source\": { \"password\": \"new password\" } } ";
            var setRequest2 =
                new SetValueRequest(configId2, null, JsonDocument.Parse(sqlPassword));
            await Configinator.SetValueAsync(setRequest2);
            var path = $"/{Organization.OrganizationId.Id}/{TestRealm1}/{TestConfigurationSection1}/dev-allard";
            var value = (await ConfigStore.GetValueAsync(path)).Value.ConvertToString();
            value.Should().Be(JsonDocument.Parse(sqlPassword).ConvertToString());

            // test is no longer valid because RAW has been eliminated.
            // fix the assert to look at the layers
        }

        [Fact]
        public async Task SetPropertyWithinDoc()
        {
            // setup - write an entire config section at once.
            var file = TestUtility.GetFile("FullDocumentPasses.json");
            var configId = new ConfigurationId(Organization.OrganizationId.Id, TestRealm1,
                TestConfigurationSection1, "dev");
            var setRequest1 =
                new SetValueRequest(configId, null, JsonDocument.Parse(file));
            var setResponse1 = await Configinator.SetValueAsync(setRequest1);
            setResponse1.Success.Should().BeTrue();

            // confirm that the value saved correctly.
            // it only checks sa. this is really just demonstrating
            // that it starts as SA
            var getRequest1 = new GetValueRequest(configId);
            var getResponse1 = await Configinator.GetValueAsync(getRequest1);
            getResponse1.Value.RootElement.GetProperty("sql-source").GetProperty("user-id").GetString().Should()
                .Be("sa");
            getResponse1.Value.RootElement.GetProperty("sql-source").GetProperty("password").GetString().Should()
                .Be("password");
            getResponse1.Value.RootElement.GetProperty("sql-source").GetProperty("host").GetString().Should()
                .Be("localhost");

            // overwrite the user-id value.
            var setRequest2 = new SetValueRequest(configId, "/sql-source/user-id",
                JsonDocument.Parse("\"yay!\""));
            var setResponse2 = await Configinator.SetValueAsync(setRequest2);
            setResponse2.Success.Should().BeTrue();

            // get the entire cs and see the value is set
            var getResponse2 = await Configinator.GetValueAsync(getRequest1);
            getResponse2.Value.RootElement.GetProperty("sql-source").GetProperty("user-id").GetString().Should()
                .Be("yay!");

            // the other values shouldn't have changed.
            getResponse2.Value.RootElement.GetProperty("sql-source").GetProperty("password").GetString().Should()
                .Be("password");
            getResponse2.Value.RootElement.GetProperty("sql-source").GetProperty("host").GetString().Should()
                .Be("localhost");

            // get the single value
            var getRequest3 = new GetValueRequest(configId, "sql-source/user-id");
            var getResponse3 = await Configinator.GetValueAsync(getRequest3);
            getResponse3.Value.RootElement.ValueKind.Should().Be(JsonValueKind.String);
            getResponse3.Value.RootElement.GetString().Should().Be("yay!");
        }

        [Fact]
        public void ValidateStraightLine()
        {
            // y : z
            var chains = Configinator.GetHabitatDescendantChains(new HabitatId("y"), CreateTestHabitats());
            chains.Count.Should().Be(1);
            var y = chains.Single();

            // 2 items on the list: y and z
            y.Count.Should().Be(2);

            // first item is y - the base
            y.First().HabitatId.Id.Should().Be("y");

            // last item is z - the child
            y.Last().HabitatId.Id.Should().Be("z");
        }

        [Fact]
        public void MultipleChainsFromTop()
        {
            // a : b : c
            // a : b : d
            // a : b : e : f
            // a : g : h
            // a : g : i : k 
            // a : g : u
            var chains = Configinator.GetHabitatDescendantChains(new HabitatId("a"), CreateTestHabitats());
            chains.Count.Should().Be(6);
        }

        [Fact]
        public void MultipleChainsFromSecondLevel()
        {
            // b : c
            // b : d
            // b : e : f
            var chains = Configinator.GetHabitatDescendantChains(new HabitatId("b"), CreateTestHabitats());
            chains.Count.Should().Be(3);
        }

        private static List<IHabitat> CreateTestHabitats()
        {
            //   A -------------------- X ------ Q
            //  a                       x        q
            //  b           g           y
            //  c d e      h i u        z     
            //      f        k


            return new TestDataBuilder()
                // a tree
                .Add("a", null)
                .Add("b", "a")
                .Add("c", "b")
                .Add("d", "b")
                .Add("e", "b")
                .Add("f", "e")
                .Add("g", "a")
                .Add("h", "g")
                .Add("i", "g")
                .Add("u", "g")
                .Add("k", "i")

                // x tree
                .Add("x", null)
                .Add("y", "x")
                .Add("z", "y")

                // q, stand alone
                .Add("q", null)
                .Values;
        }

        [Fact]
        public void Tree()
        {
            var id = new HabitatId("i");
            var tree = Configinator.GetHabitatTree(id, CreateTestHabitats());
            testOutputHelper.WriteLine("");
        }

        /// <summary>
        ///     Get the chains of a habitat that doesn't have a parent
        ///     nor children
        /// </summary>
        [Fact]
        public void GetChainSingle()
        {
            var chains = Configinator.GetHabitatDescendantChains(new HabitatId("q"), CreateTestHabitats());
            chains.Count.Should().Be(1);
            chains[0].Count.Should().Be(1);
            chains[0][0].HabitatId.Should().Be(new HabitatId("q"));
            chains[0][0].BaseHabitat.Should().BeNull();
        }

        private class TestDataBuilder
        {
            private readonly Dictionary<string, IHabitat> values = new();

            public List<IHabitat> Values => values.Values.ToList();

            public TestDataBuilder Add(string habitatId, string baseHabitatId)
            {
                var baseHabitat = baseHabitatId == null
                    ? null
                    : values[baseHabitatId];
                var h = new DummyHabitat(habitatId, baseHabitat);
                values.Add(habitatId, h);
                return this;
            }
        }


        public record DummyHabitat : IHabitat
        {
            public DummyHabitat(string id, IHabitat baseHabitat)
            {
                Realm = null;
                HabitatId = new HabitatId(id);
                BaseHabitat = baseHabitat;
            }

            public IRealm Realm { get; }
            public HabitatId HabitatId { get; }
            public IHabitat BaseHabitat { get; }
        }
    }
}