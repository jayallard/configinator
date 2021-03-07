namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        ConfigurationSectionId ConfigurationSectionId,
        string Path, 
        SchemaTypeId SchemaTypeId,
        string Description);
}