namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        ConfigurationSectionId ConfigurationSectionId, 
        string Path, SchemaType SchemaType,
        string Description);
}
