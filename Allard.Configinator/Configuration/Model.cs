using System.Collections.Generic;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Configuration
{
    public record ConfigurationSectionId(string Namespace, string Name);
    public record ConfigurationSectionValue(ConfigurationSection Section, string ETag, string Value);
    public record ConfigurationSection(ConfigurationSectionId Id, string Path, SchemaParser.ObjectSchemaType Type, string Description);
    public record Space(string Name, string Description, IReadOnlySet<string> Bases);

    public record ConfigurationNamespace(string Name, IReadOnlyCollection<ConfigurationSection> ConfigurationSections);

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