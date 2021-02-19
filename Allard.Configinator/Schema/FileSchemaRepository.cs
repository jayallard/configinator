using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    /// <summary>
    /// Retrieves schema yaml from files.
    /// </summary>
    public class FileSchemaRepository : ISchemaRepository
    {
        private readonly string schemaFolder;

        /// <summary>
        /// Initializes a new instance of the fileSchemaRepository class.
        /// </summary>
        /// <param name="schemaFolder">The folder that contains the schema files.</param>
        public FileSchemaRepository(string schemaFolder)
        {
            this.schemaFolder = schemaFolder ?? throw new ArgumentNullException(nameof(schemaFolder));
        }

        /// <summary>
        /// Retrieve schema yaml.
        /// The name of the file will be id.yml.
        /// The id in the file must match the file name.
        /// If the file doesn't exist, it will throw an exception.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<YamlMappingNode> GetSchemaYaml(string nameSpace)
        {
            var fileName = Path.Combine(schemaFolder, nameSpace + ".yml");
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Schema file doesn't exist: " + Path.GetFileName(fileName), fileName);
            }

            var yaml = await File.ReadAllTextAsync(fileName);
            using var reader = new StringReader(yaml);
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            if (yamlStream.Documents.Count != 1)
            {
                throw new InvalidOperationException("Schema file should have one document. It has " +
                                                    yamlStream.Documents.Count);
            }

            return (YamlMappingNode)yamlStream.Documents[0].RootNode;
        }
    }
}