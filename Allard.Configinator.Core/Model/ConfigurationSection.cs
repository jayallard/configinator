using System.Collections.Generic;

namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        Realm Realm,
        SectionId SectionId,
        IReadOnlyCollection<SchemaTypeProperty> Properties,
        string Description);
}