using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    /// <summary>
    /// Retrieves schema yaml from files.
    /// </summary>
    public class FileSchemaMetaRepository : ISchemaMetaRepository
    {
        private readonly string schemaFolder;

        /// <summary>
        /// Initializes a new instance of the fileSchemaRepository class.
        /// </summary>
        /// <param name="schemaFolder">The folder that contains the schema files.</param>
        public FileSchemaMetaRepository(string schemaFolder)
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
            var yaml = (await YamlUtility.GetYamlFromFile(schemaFolder, nameSpace + ".yml")).ToList();
            if (yaml.Count != 1)
            {
                throw new InvalidOperationException("Schema file should have one document. It has " +
                                                    yaml.Count);
            }

            return (YamlMappingNode) yaml[0].RootNode;
        }

        public Task<IReadOnlySet<string>> GetNamespaces()
        {
            return Task.Run<IReadOnlySet<string>>(() =>
                Directory
                    .GetFiles(schemaFolder, "*.yml")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .ToHashSet());
        }
    }
}