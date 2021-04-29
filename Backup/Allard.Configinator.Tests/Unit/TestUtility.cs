using System;
using System.IO;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit
{
    public static class TestUtility
    {
        public static string SchemaFolder { get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas");

        public static string SchemaExpectedResolutionFolder { get; } = Path.Combine(SchemaFolder, "ExpectedResolution");

        public static async Task<YamlNode> GetExpectedResolution(string schemaId)
        {
            var file = Path.Combine(SchemaExpectedResolutionFolder, schemaId + "-results.yml");
            return await GetYamlFromFile(file);
        }
        public static ISchemaRepository CreateSchemaRepository() => new FileSchemaRepository(SchemaFolder);

        public static SchemaParser CreateSchemaParser() => new SchemaParser(CreateSchemaRepository());

        private static async Task<YamlNode> GetYamlFromFile(string fileName)
        {
            var yaml = await File.ReadAllTextAsync(fileName);
            var reader = new StringReader(yaml);
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            if (yamlStream.Documents.Count != 1)
            {
                throw new InvalidOperationException("Schema file should have one document. It has " +
                                                    yamlStream.Documents.Count);
            }

            return yamlStream.Documents[0].RootNode;
        }
    }
}