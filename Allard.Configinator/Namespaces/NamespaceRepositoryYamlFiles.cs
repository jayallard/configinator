using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                .Select(async f => await YamlUtility.GetYamlFromFile(f).ConfigureAwait(false))
                .ToList();
            await Task.WhenAll(files).ConfigureAwait(false);
            return files
                .SelectMany(f => f.Result)
                .Where(f => f.RootNode.AsString("$$doc") == "namespace")
                .Select(f => NamespaceYamlDeserializer.Deserialize(f.RootNode.AsMap()));
        }
    }
}