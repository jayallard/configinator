using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     Retrieves schema yaml from files.
    /// </summary>
    public class FileSchemaRepository : ISchemaRepository
    {
        private readonly string schemaFolder;

        /// <summary>
        ///     Initializes a new instance of the fileSchemaRepository class.
        /// </summary>
        /// <param name="schemaFolder">The folder that contains the schema files.</param>
        public FileSchemaRepository(string schemaFolder)
        {
            this.schemaFolder = string.IsNullOrWhiteSpace(schemaFolder)
                ? throw new ArgumentNullException(nameof(schemaFolder))
                : schemaFolder;
        }

        /// <summary>
        ///     Retrieve schema yaml.
        ///     The name of the file will be id.yml.
        ///     The id in the file must match the file name.
        ///     If the file doesn't exist, it will throw an exception.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<YamlMappingNode> GetSchemaYaml(string nameSpace)
        {
            nameSpace = string.IsNullOrWhiteSpace(nameSpace)
                ? throw new ArgumentNullException(nameof(nameSpace))
                : nameSpace;

            var yaml = (await YamlUtility.GetYamlFromFile(schemaFolder, nameSpace + ".yml")).ToList();
            if (yaml.Count != 1)
                throw new InvalidOperationException("Schema file should have one document. It has " +
                                                    yaml.Count);

            return (YamlMappingNode) yaml[0].RootNode;
        }

        public async Task<IEnumerable<ModelDto.TypeDto>> GetSchemaTypes()
        {
            var yamlTasks = Directory
                .GetFiles(schemaFolder, "*.yml")
                .Select(async f => await YamlUtility.GetYamlFromFile(f));
            var yamlDocs = await Task.WhenAll(yamlTasks);
            return yamlDocs.SelectMany(YamlSchemaDeserializer.Deserialize);
        }

        public async Task<IEnumerable<ModelDto.TypeDto>> GetTypes(string nameSpace)
        {
            nameSpace = string.IsNullOrWhiteSpace(nameSpace)
                ? throw new ArgumentNullException(nameof(nameSpace))
                : nameSpace;
            var yaml = await YamlUtility.GetYamlFromFile(schemaFolder, nameSpace + ".yml");
            return YamlSchemaDeserializer
                .Deserialize(yaml)
                .Where(y => y.Namespace == nameSpace);
        }
    }
}