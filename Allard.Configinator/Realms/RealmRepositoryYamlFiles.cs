using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Allard.Configinator.Realms
{
    public class RealmRepositoryYamlFiles : IRealmRepository
    {
        private readonly string realmFolder;

        public RealmRepositoryYamlFiles(string realmFolder)
        {
            this.realmFolder = realmFolder.EnsureValue(nameof(realmFolder));
        }

        public async Task<IEnumerable<RealmStorageDto>> GetRealms()
        {
            var files = Directory
                .GetFiles(realmFolder, "*.yml")
                .Select(async f => await YamlUtility.GetYamlFromFile(f).ConfigureAwait(false))
                .ToList();
            await Task.WhenAll(files).ConfigureAwait(false);
            return files
                .SelectMany(f => f.Result)
                .Where(f => f.RootNode.AsString("$$doc") == "realm")
                .Select(f => RealmYamlDeserializer.Deserialize(f.RootNode.AsMap()));
        }
    }
}