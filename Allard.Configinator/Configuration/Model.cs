namespace Allard.Configinator.Configuration
{
    public record ConfigStoreValue(string Path, string ETag, string Value)
    {
        // todo: probably get rid of this
        public ConfigStoreValue SetValue(string value)
        {
            return this with {Value = value};
        }
    }
}