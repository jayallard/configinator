using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;

namespace Allard.Configinator
{
    public record ConfigurationSectionId(string Namespace, string Name);

    public record ConfigurationSection(ConfigurationSectionId Id, string Path, ObjectSchemaType Type,
        string Description);

    public record ConfigurationSectionValue(ConfigurationId Id, string Etag, string Value)
    {
        public ConfigurationSectionValue SetValue(string value)
        {
            return this with {Value = value};
        }
    }

    public record ConfigurationNamespace(string Name, IReadOnlyCollection<ConfigurationSection> ConfigurationSections)
    {
        // todo: validate all sections are the proper namespace.
        // todo: dictionary by name
        public ConfigurationSection GetConfigurationSection(string name)
        {
            return ConfigurationSections.Single(cs => cs.Id.Name == name);
        }
    }
}