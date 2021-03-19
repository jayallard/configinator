using System.Collections.Generic;

namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        Realm Realm,
        SectionId SectionId,
        string Path,
        IReadOnlyCollection<SchemaTypeProperty> Properties,
        string Description);
}