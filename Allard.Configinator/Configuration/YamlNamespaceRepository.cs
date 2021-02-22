using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Configuration
{
    public class YamlNamespaceRepository : INamespaceRepository
    {
        private readonly string namespaceFolder;

        public YamlNamespaceRepository(string namespaceFolder)
        {
            this.namespaceFolder = string.IsNullOrWhiteSpace(namespaceFolder)
                ? throw new ArgumentNullException(nameof(namespaceFolder))
                : namespaceFolder;
        }

        public async Task<IEnumerable<NamespaceDto>> GetNamespaces()
        {
            var files = Directory
                .GetFiles(namespaceFolder, "*.yml")
                .Select(async f =>
                {
                    var yaml = (await YamlUtility.GetYamlFromFile(f))
                        .Single()
                        .RootNode
                        .AsMap();
                    return Deserializers.DeserializeNamespace(yaml);
                })
                .ToList();
            await Task.WhenAll(files);
            return files.Select(f => f.Result);
        }
    }
}