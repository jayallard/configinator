using System.Collections.Generic;

namespace Allard.Configinator.Realms
{
    public class RealmStorageDto
    {
        public string Name { get; set; }

        public List<ConfigurationSectionStorageDto> ConfigurationSections { get; set; }

        public class ConfigurationSectionStorageDto
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }
    }
}