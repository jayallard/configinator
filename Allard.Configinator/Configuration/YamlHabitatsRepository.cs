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
            return (await YamlUtility.GetYamlFromFile(yamlFile))
                .Where(y => y.RootNode.AsString("$$doc") == "habitat")
                .SelectMany(y => Deserializers.DeserializeHabitat(y.RootNode.AsMap()));
        }
    }
}