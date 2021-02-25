namespace Allard.Configinator.Configuration
{

    public record ConfigurationId(string Habitat, string Namespace, string ConfigurationSection);

    public record ConfigurationValue(string Path, string ETag, string Value)
    {
        public ConfigurationValue SetValue(string value) => this with {Value = value};
    }}