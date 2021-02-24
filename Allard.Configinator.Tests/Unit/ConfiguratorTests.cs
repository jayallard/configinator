using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;
using FluentAssertions;
using FluentAssertions.Xml;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

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
            var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FullSetup");
            var schemasFolder = Path.Combine(baseFolder, "Schemas");
            var namespaceFolder = Path.Combine(baseFolder, "Namespaces");
            var habitatsFile = Path.Combine(baseFolder, "Habitats", "habitats.yml");

            var configStore = new MemoryConfigStore();
            var parser = new SchemaService(new FileSchemaRepository(schemasFolder));
            var spaceRepo = new YamlHabitatsRepository(habitatsFile);
            var namespaceRepo = new YamlNamespaceRepository(namespaceFolder);
            return new Configinator(
                parser,
                configStore,
                spaceRepo,
                namespaceRepo);
        }

        [Fact]
        public async Task ValueOverrides()
        {
            // dev-jay2 is based on dev-jay and development.
            // set values for dev-jay and development,
            // and see that overrides work.
            const string dev = "{ \"host\": \"dev\", \"user-id\": \"boom\", \"password\": \"dev-pw\" }";
            const string jay1 = "{ \"host\": \"jay1\" }";
            const string jay2 = "{ \"host\": \"jay2\", \"password\": \"jay2-pw\" }";

            var configinator = CreateConfiginator();
            await CreateValueAsync(configinator, "development", "domain-a", "service-1", dev);
            await CreateValueAsync(configinator, "dev-jay1", "domain-a", "service-1", jay1);
            await CreateValueAsync(configinator, "dev-jay2", "domain-a", "service-1", jay2);

            var value = await configinator.GetValueAsync("dev-jay2", "domain-a", "service-1");
            var expectedValue = "{ \"host\": \"jay2\", \"password\": \"jay2-pw\", \"user-id\": \"boom\" }";

            Assert.True(JToken.DeepEquals(
                JToken.Parse(value.Value),
                JToken.Parse(expectedValue)
            ));
        }

        private static async Task CreateValueAsync(Configinator configinator, string habitat, string nameSpace,
            string configSection,
            string value)
        {
            var v = await configinator.GetValueAsync(habitat, nameSpace, configSection);
            v.SetValue(value);
            await configinator.SetValueAsync(v);
        }

        [Fact]
        public async Task Proto()
        {
            var configinator = CreateConfiginator();
            // -----------------------------------------
            // namespaces
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Namespaces:");
            var namespaces = (await configinator.GetNamespacesAsync()).ToList();
            foreach (var ns in namespaces)
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
            var habitats = (await configinator.GetHabitats()).ToList();
            foreach (var space in habitats) testOutputHelper.WriteLine("\t" + space.Name);

            // -----------------------------------------
            // get/set config
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Get/Set Values:");
            var value = await configinator.GetValueAsync("dev-jay2", "domain-a", "service-1");
            value.Value.Should().BeNull();
            value.SetValue("{ \"hello\": \"world\" }");
            testOutputHelper.WriteLine("\tNo existing value, as expected");

            await configinator.SetValueAsync(value);
            testOutputHelper.WriteLine("\tSet value");

            var value2 = await configinator.GetValueAsync("dev-jay2", "domain-a", "service-1");
            Assert.True(JToken.DeepEquals(JToken.Parse(value.Value), JToken.Parse(value2.Value)));
            testOutputHelper.WriteLine("\tRead value: " + value2.Value);
        }
    }
}