using System;
using System.IO;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit
{
    public static class TestUtility
    {
        /// <summary>
        ///     Gets the folder that has the test schemas.
        /// </summary>
        public static string SchemaFolder { get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "Schemas");

        /// <summary>
        ///     Gets the folder that has the test resolutions.
        ///     The resolutions define what a resolved schema should look like.
        ///     It defines the "expected" values as yaml so they don't have
        ///     to be coded.
        /// </summary>
        public static string SchemaExpectedResolutionFolder { get; } = Path.Combine(SchemaFolder, "ExpectedResolution");

        /// <summary>
        ///     Get the resolution file for a schema.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        public static async Task<YamlNode> GetExpectedResolution(string schemaId)
        {
            var file = Path.Combine(SchemaExpectedResolutionFolder, schemaId + "-results.yml");
            return await GetYamlFromFile(file);
        }

        /// <summary>
        ///     Creates a repository object that uses the schema folder.
        /// </summary>
        /// <returns></returns>
        public static ISchemaRepository CreateSchemaRepository()
        {
            return new FileSchemaRepository(SchemaFolder);
        }

        /// <summary>
        ///     Creates a schema parser using the schema folder.
        /// </summary>
        /// <returns></returns>
        public static ISchemaService CreateSchemaParser()
        {
            return new SchemaService(CreateSchemaRepository());
        }

        /// <summary>
        ///     Loads yaml from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<YamlNode> GetYamlFromFile(string fileName)
        {
            var yaml = await File.ReadAllTextAsync(fileName);
            var reader = new StringReader(yaml);
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            if (yamlStream.Documents.Count != 1)
                throw new InvalidOperationException("Schema file should have one document. It has " +
                                                    yamlStream.Documents.Count);

            return yamlStream.Documents[0].RootNode;
        }
    }
}