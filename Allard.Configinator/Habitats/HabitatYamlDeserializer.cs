using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Habitats
{
    public static class HabitatYamlDeserializer
    {
        // TODO: switch to DTO
        public static IEnumerable<Habitat> Deserialize(YamlMappingNode habitats)
        {
            habitats.EnsureValue(nameof(habitats));
            return habitats
                .AsMap("habitats")
                .Children
                .Select(s =>
                {
                    var (key, value) = s;
                    var habitatName = (string) key;
                    if (value is YamlScalarNode)
                        // no children, so nothing else to do.
                        return new Habitat(habitatName, null, new HashSet<string>());

                    var node = value.AsMap();
                    var description = node.AsString("description");
                    var bases = node.AsStringHashSet("bases");
                    return new Habitat(habitatName, description, bases);
                });
        }
    }
}