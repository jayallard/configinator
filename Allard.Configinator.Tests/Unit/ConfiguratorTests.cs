using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Habitats;
using Allard.Configinator.Realms;
using Allard.Configinator.Schema;
using Allard.Configinator.Schema.Validator;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Tests.Unit
{
    public class ConfiguratorTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ConfiguratorTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        private Configinator CreateConfiginator()
        {
            var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "FullSetup");
            var habitatsFile = Path.Combine(baseFolder, "habitats.yml");

            var configStore = new MemoryConfigStore();
            var schemaService = new SchemaService(new SchemaRepositoryYamlFiles(baseFolder),
                new SchemaValidator(new ValidatorFactoryServices()));
            var habitatService = new HabitatService(new HabitatsRepositoryYamlFile(habitatsFile));
            var realmService = new RealmService(new RealmRepositoryYamlFiles(baseFolder), schemaService);
            return new Configinator(
                configStore,
                habitatService,
                realmService);
        }

        [Fact]
        public async Task MergeValues()
        {
            // habitats.yml defines that dev-jay2 overrides development and dev-jay1.
            // write values to all 3, then get the values from dev-jay2.
            // see that the returned values it he 
            const string dev =
                "{ \"host\": \"dev\", \"user-id\": \"boom\", \"password\": \"dev-pw\", \"remove-me\": \"please\" }";
            const string jay1 = "{ \"host\": \"jay1\", \"remove-me\": null }";
            const string jay2 = "{ \"host\": \"jay2\", \"password\": \"jay2-pw\" }";

            // host is set in jay2
            // password is set in jay2
            // userid is from dev (jay1 doesn't have it)
            // remove-me is defined in dev, but nulled-out in jay1, so doesn't exist in jay-2.
            const string expectedMergeResult =
                "{ \"host\": \"jay2\", \"password\": \"jay2-pw\", \"user-id\": \"boom\" }";

            var configinator = CreateConfiginator();
            await CreateValueAsync(configinator, "development", "domain-a", "service-1", dev);
            await CreateValueAsync(configinator, "dev-jay1", "domain-a", "service-1", jay1);
            await CreateValueAsync(configinator, "dev-jay2", "domain-a", "service-1", jay2);

            var idToGet = new ConfigurationId("dev-jay2", "domain-a", "service-1");
            var value = await configinator.Configuration.Get(idToGet);

            Assert.True(JToken.DeepEquals(
                JToken.Parse(value.ResolvedValue),
                JToken.Parse(expectedMergeResult)
            ));
        }

        private static async Task CreateValueAsync(Configinator configinator, string habitat, string realm,
            string configSection,
            string value)
        {
            var v = await configinator.Configuration.Get(new ConfigurationId(habitat, realm, configSection));
            await configinator.Configuration.Set(v.ToSetter(value));
        }

        [Fact]
        public async Task Proto()
        {
            var configinator = CreateConfiginator();
            // -----------------------------------------
            // realms
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Realms:");
            var realms = (await configinator.Realms.All()).ToList();
            foreach (var ns in realms)
            {
                testOutputHelper.WriteLine(ns.Name);
                foreach (var cs in ns.ConfigurationSections)
                {
                    testOutputHelper.WriteLine("\t" + cs.Id.Name);
                    testOutputHelper.WriteLine("\t\tPath = " + cs.Path);
                    testOutputHelper.WriteLine("\t\tType Id = " + cs.Type.SchemaTypeId.FullId);
                }
            }

            // -----------------------------------------
            // habitats
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Habitats:");
            var habitats = (await configinator.Habitats.All()).ToList();
            foreach (var space in habitats) testOutputHelper.WriteLine("\t" + space.Name);

            // -----------------------------------------
            // get/set config
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Get/Set Values:");
            var value = await configinator.Configuration.Get(new ConfigurationId("dev-jay2", "domain-a", "service-1"));
            value.ResolvedValue.Should().BeNull();
            var setter = value.ToSetter("{ \"hello\": \"world\" }");
            testOutputHelper.WriteLine("\tNo existing value, as expected");

            await configinator.Configuration.Set(setter);
            testOutputHelper.WriteLine("\tSet value");

            var value2 = await configinator.Configuration.Get(new ConfigurationId("dev-jay2", "domain-a", "service-1"));
            Assert.True(JToken.DeepEquals(JToken.Parse(setter.Value), JToken.Parse(value2.ResolvedValue)));
            testOutputHelper.WriteLine("\tRead value: " + value2.ResolvedValue);
        }
    }
}