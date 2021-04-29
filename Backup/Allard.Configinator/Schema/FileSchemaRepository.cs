using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public class FileSchemaRepository : ISchemaRepository
    {
        private readonly string schemaFolder;

        public FileSchemaRepository(string schemaFolder)
        {
            this.schemaFolder = schemaFolder;
        }

        public async Task<YamlMappingNode> GetSchema(string id)
        {
            var fileName = Path.Combine(schemaFolder, id + ".yml");
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Schema file doesn't exist: " + Path.GetFileName(fileName), fileName);
            }

            var yaml = await File.ReadAllTextAsync(fileName);
            var reader = new StringReader(yaml);
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