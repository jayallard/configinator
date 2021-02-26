using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Namespaces
{
    public static class NamespaceYamlDeserializer
    {
        public static NamespaceDto Deserialize(YamlMappingNode namespaceNode)
        {
            namespaceNode.EnsureValue(nameof(namespaceNode));
            return new NamespaceDto
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
}