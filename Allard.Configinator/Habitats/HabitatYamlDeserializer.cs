using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Schema;
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
                    var name = (string) s.Key;
                    if (s.Value is YamlScalarNode)
                        // no children, so nothing else to do.
                        return new Habitat(name, null, new HashSet<string>());

                    var node = s.Value.AsMap();
                    var description = node.AsString("description");
                    var bases = node.AsStringHashSet("bases");
                    return new Habitat(name, description, bases);
                });
        }
    }
}