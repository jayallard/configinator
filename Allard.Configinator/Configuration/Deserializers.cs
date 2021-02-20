using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Schema;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Configuration
{
    public static class Deserializers
    {
        // TODO: switch to DTO
        public static IEnumerable<Space> DeserializeSpace(YamlMappingNode spaces)
        {
            return spaces
                .AsMap("spaces")
                .Children
                .Select(s =>
                {
                    var name = (string) s.Key;
                    if (s.Value is YamlScalarNode)
                    {
                        // no children, so nothing else to do.
                        return new Space(name, null, new HashSet<string>());
                    }

                    var node = s.Value.AsMap();
                    var description = node.AsString("description");
                    var bases = node.AsStringHashSet("bases");
                    return new Space(name, description, bases);
                });
        }


        public static NamespaceDto DeserializeNamespace(YamlMappingNode namespaceNode) => new()
        {
            Name = namespaceNode.AsString("namespace"),
            Sections = namespaceNode
                .AsMap("configuration-sections")
                .Children
                .Select(section => new NamespaceDto.ConfigurationSection
                {
                    Description = section.Value.AsString("description"),
                    Name = (string) section.Key,
                    Type = section.Value.AsString("type"),
                    Path = section.Value.AsString("path")
                }).ToList()
        };
    }
}