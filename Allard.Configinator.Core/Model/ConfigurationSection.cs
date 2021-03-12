namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        Realm Realm,
        SectionId SectionId,
        string Path,
        SchemaType SchemaType,
        string Description);
}