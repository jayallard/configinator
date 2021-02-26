namespace Allard.Configinator.Configuration
{
    public record ConfigurationId(string Habitat, string Realm, string ConfigurationSection);

    public record ConfigurationValue(string Path, string ETag, string Value)
    {
        public ConfigurationValue SetValue(string value)
        {
            return this with {Value = value};
        }
    }
}