using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Tests.Unit
{
    public class ProtoTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ProtoTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Proto()
        {
            var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FullSetup");
            var schemasFolder = Path.Combine(baseFolder, "Schemas");
            var namespaceFolder = Path.Combine(baseFolder, "Namespaces");
            var spacesFile = Path.Combine(baseFolder, "Spaces", "spaces.yml");
            
            var configStore = new MemoryConfigStore();
            var parser = new SchemaParser(new FileSchemaMetaRepository(schemasFolder));
            var spaceRepo = new YamlSpacesRepository(spacesFile);
            var namespaceRepo = new YamlNamespaceRepository(namespaceFolder);
            var configinator = new ConfiginatorService(
                parser,
                configStore,
                spaceRepo,
                namespaceRepo);

            // -----------------------------------------
            // namespaces
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Namespaces:");
            var namespaces = (await configinator.GetNamespaces()).ToList();
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
            // spaces
            // -----------------------------------------
            testOutputHelper.WriteLine("--------------------------------------------------------");
            testOutputHelper.WriteLine("Spaces:");
            var spaces = (await configinator.GetSpaces()).ToList();
            foreach (var space in spaces)
            {
                testOutputHelper.WriteLine("\t" + space.Name);
            }
            
            // -----------------------------------------
            // set config
            // -----------------------------------------
            var config = "{}";
            var value = new ConfigurationSectionValue()
            configinator.Save()
            
        }
    }
}