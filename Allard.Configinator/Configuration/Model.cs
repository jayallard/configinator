using System.Collections.ObjectModel;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Configuration
{
    public record ConfigurationSectionId(string Namespace, string Name);
    public record ConfigurationSectionValue(ConfigurationSection Id, string ETag, string Value);

    public record ConfigurationSection(ConfigurationSectionId Id, string Path, SchemaType Type, string Description);
    
}