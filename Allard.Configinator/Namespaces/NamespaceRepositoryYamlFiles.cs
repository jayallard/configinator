using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Namespaces
{
    public class NamespaceRepositoryYamlFiles : INamespaceRepository
    {
        private readonly string namespaceFolder;

        public NamespaceRepositoryYamlFiles(string namespaceFolder)
        {
            this.namespaceFolder = namespaceFolder.EnsureValue(nameof(namespaceFolder));
        }

        public async Task<IEnumerable<NamespaceDto>> GetNamespaces()
        {
            var files = Directory
                .GetFiles(namespaceFolder, "*.yml")
                .Select(async f => await YamlUtility.GetYamlFromFile(f))
                .ToList();
            await Task.WhenAll(files);
            return files
                .SelectMany(f => f.Result)
                .Where(f => f.RootNode.AsString("$$doc") == "namespace")
                .Select(f => NamespaceYamlDeserializer.Deserialize(f.RootNode.AsMap()));
        }
    }
}