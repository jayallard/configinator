using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Configuration
{
    public class YamlSpacesRepository : ISpaceRepository
    {
        private readonly string yamlFile;

        public YamlSpacesRepository(string yamlFile)
        {
            this.yamlFile = string.IsNullOrWhiteSpace(yamlFile)
                ? throw new ArgumentNullException(nameof(yamlFile))
                : yamlFile;
        }

        public async Task<IEnumerable<Space>> GetSpaces()
        {
            var yaml = (await YamlUtility.GetYamlFromFile(yamlFile))
                .Single()
                .RootNode
                .AsMap();
            return Deserializers.DeserializeSpace(yaml);
        }

        public Task<Space> GetSpace(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}