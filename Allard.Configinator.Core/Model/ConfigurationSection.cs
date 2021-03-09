namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        Realm Realm,
        ConfigurationSectionId ConfigurationSectionId,
        string Path,
        SchemaTypeId SchemaTypeId,
        string Description);
}