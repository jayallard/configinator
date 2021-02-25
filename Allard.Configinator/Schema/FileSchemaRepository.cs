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

        public async Task<IEnumerable<ModelDto.TypeDto>> GetSchemaTypes()
        {
            var yamlTasks = Directory
                .GetFiles(schemaFolder, "*.yml")
                .Select(async f => await YamlUtility.GetYamlFromFile(f));
            var yamlDocs = await Task.WhenAll(yamlTasks);
            return yamlDocs
                .SelectMany(YamlSchemaDeserializer.Deserialize);
        }
    }
}