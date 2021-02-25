using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Habitats
{
    public class HabitatsRepositoryYamlFile : IHabitatRepository
    {
        private readonly string yamlFile;

        public HabitatsRepositoryYamlFile(string yamlFile)
        {
            this.yamlFile = yamlFile.EnsureValue(nameof(yamlFile));
        }

        public async Task<IEnumerable<Habitat>> GetHabitats()
        {
            return (await YamlUtility.GetYamlFromFile(yamlFile))
                .Where(y => y.RootNode.AsString("$$doc") == "habitat")
                .SelectMany(y => HabitatYamlDeserializer.Deserialize(y.RootNode.AsMap()));
        }
    }
}