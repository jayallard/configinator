using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.DocumentMerger;

namespace Allard.Configinator.Core.Infrastructure
{
    public record ConfigStoreValue(string Path, JsonDocument Value, bool Exists);

    public record SetConfigStoreValueRequest(string Path, JsonDocument Value);

    public record ConfigurationId(string OrganizationId, string RealmId, string SectionId, string HabitatId);

    public record ConfigurationValue(ConfigurationId Id, bool Exists, JsonDocument ResolvedValue, List<MergedProperty> Properties);
}