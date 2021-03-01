using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Schema;

namespace Allard.Configinator
{
    public record ConfigurationId(string Habitat, string Realm, string ConfigurationSection);

    public record ConfigurationValue(ConfigurationId Id, string Etag, string ResolvedValue,
        IEnumerable<ConfigurationValue> Bases)
    {
        public ConfigurationValueSetter ToSetter(string value)
        {
            return new(Id, Etag, value);
        }
    }

    public record ConfigurationValueSetter(ConfigurationId Id, string LastEtag, string Value)
    {
    }


    public record ConfigurationSection(ConfigurationSectionId Id, string Path, ObjectSchemaType Type,
        string Description);

    public record ConfigurationSectionId(string Realm, string Name);

    public record Realm(string Name, IReadOnlyCollection<ConfigurationSection> ConfigurationSections)
    {
        // todo: validate all sections are the proper realm.
        // todo: dictionary by name
        public ConfigurationSection GetConfigurationSection(string name)
        {
            return ConfigurationSections.Single(cs => cs.Id.Name == name);
        }
    }
}