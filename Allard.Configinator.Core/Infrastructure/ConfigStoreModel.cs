using System.Collections.Generic;
using Allard.Configinator.Core.DocumentMerger;

namespace Allard.Configinator.Core.Infrastructure
{
    public record ConfigStoreValue(string Path, string ETag, string Value);
    public record ConfigurationId(string Habitat, string Realm, string ConfigurationSection);
    public record ConfigurationValue(ConfigurationId Id, string Etag, string ResolvedValue);
}