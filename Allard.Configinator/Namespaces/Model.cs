using System.Collections.Generic;

namespace Allard.Configinator.Namespaces
{
    public class NamespaceDto
    {
        public string Name { get; set; }

        public List<ConfigurationSection> Sections { get; set; }

        public class ConfigurationSection
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }
    }
}