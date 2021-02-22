using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Configuration
{
    public class YamlHabitatsRepository : IHabitatRepository
    {
        private readonly string yamlFile;

        public YamlHabitatsRepository(string yamlFile)
        {
            this.yamlFile = string.IsNullOrWhiteSpace(yamlFile)
                ? throw new ArgumentNullException(nameof(yamlFile))
                : yamlFile;
        }

        public async Task<IEnumerable<Habitat>> GetHabitats()
        {
            var yaml = (await YamlUtility.GetYamlFromFile(yamlFile))
                .Single()
                .RootNode
                .AsMap();
            return Deserializers.DeserializeHabitat(yaml);
        }

        public Task<Habitat> GetSpace(string name)
        {
            throw new NotImplementedException();
        }
    }
}