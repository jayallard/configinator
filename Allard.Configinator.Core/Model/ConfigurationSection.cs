using System.Collections.Generic;

namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(
        Realm Realm,
        SectionId SectionId,
        IReadOnlyCollection<SchemaTypePropertyExploded> Properties,
        string Description)
    {
        public SchemaTypeProperty Find(string path)
        {
            return null;
        }
    }
}