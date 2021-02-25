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
            // return (await YamlUtility.GetYamlFromFile(yamlFile))
            //     .Where(y => y.RootNode.AsString("$$doc") == "habitat")
            //     .SelectMany(y => Deserializers.DeserializeHabitat(y.RootNode.AsMap()));

            var files = Directory
                .GetFiles(namespaceFolder, "*.yml")
                .Select(async f => await YamlUtility.GetYamlFromFile(f));
            await Task.WhenAll(files);
            return files
                .SelectMany(f => f.Result)
                .Where(f => f.RootNode.AsString("$$doc") == "namespace")
                .Select(f => Deserializers.DeserializeNamespace(f.RootNode.AsMap()));
        }
    }
}