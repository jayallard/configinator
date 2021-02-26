using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     Retrieves schema yaml from files.
    /// </summary>
    public class SchemaRepositoryYamlFiles : ISchemaRepository
    {
        private readonly string schemaFolder;

        /// <summary>
        ///     Initializes a new instance of the fileSchemaRepository class.
        /// </summary>
        /// <param name="schemaFolder">The folder that contains the schema files.</param>
        public SchemaRepositoryYamlFiles(string schemaFolder)
        {
            this.schemaFolder = schemaFolder.EnsureValue(nameof(schemaFolder));
        }

        public async Task<IEnumerable<TypeDto>> GetSchemaTypes()
        {
            var yamlTasks = Directory
                .GetFiles(schemaFolder, "*.yml")
                .Select(async f => await YamlUtility.GetYamlFromFile(f).ConfigureAwait(false));
            var yamlDocs = await Task.WhenAll(yamlTasks).ConfigureAwait(false);
            return yamlDocs
                .SelectMany(YamlSchemaDeserializer.Deserialize);
        }
    }
}