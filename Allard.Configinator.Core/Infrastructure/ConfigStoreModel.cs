namespace Allard.Configinator.Core.Infrastructure
{
    public record ConfigStoreValue(string Path, string ETag, string Value);

    public record ConfigurationId(string OrganizationName, string HabitatName, string RealmName,
        string ConfigurationSectionName);

    public record ConfigurationValue(ConfigurationId Id, string Etag, string ResolvedValue);
}