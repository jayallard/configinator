using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Realms
{
    public static class RealmYamlDeserializer
    {
        public static RealmStorageDto Deserialize(YamlMappingNode realmNode)
        {
            realmNode.EnsureValue(nameof(realmNode));
            return new RealmStorageDto
            {
                Name = realmNode.AsString("name"),
                ConfigurationSections = realmNode
                    .AsMap("configuration-sections")
                    .Children
                    .Select(section => new RealmStorageDto.ConfigurationSectionStorageDto
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