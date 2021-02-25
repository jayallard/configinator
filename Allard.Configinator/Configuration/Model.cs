using System;
using System.Collections.Generic;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Configuration
{
    public record ConfigurationSectionId(string Namespace, string Name);

    public class ConfigurationSectionValue
    {
        public ConfigurationSectionValue(string habitat, string configurationSection, string eTag,
            string value)
        {
            Habitat = habitat ?? throw new ArgumentNullException(nameof(habitat));
            ConfigurationSection =
                configurationSection ?? throw new ArgumentNullException(nameof(configurationSection));
            ETag = eTag;
            Value = value;
        }

        public string Habitat { get; }
        public string ConfigurationSection { get; }
        public string ETag { get; }
        public string Value { get; private set; }

        public ConfigurationSectionValue SetValue(string value)
        {
            Value = value;
            return this;
        }
    }

    public record ConfigurationSection(ConfigurationSectionId Id, string Path, ObjectSchemaType Type,
        string Description);

    public record Habitat(string Name, string Description, IReadOnlySet<string> Bases);

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