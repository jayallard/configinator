using System.Collections.Generic;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.Infrastructure
{
    public record GetConfigurationRequest(ConfigurationId ConfigurationId);

    // todo: return actual value
    public record GetConfigurationResponse(ConfigurationId ConfigurationId, string? ETag, string ResolvedValue,
        IList<MergedProperty> PropertyDetail)
    {
        public bool Existing => PropertyDetail.Count > 0;
    }

    public record SetConfigurationRequest(ConfigurationId ConfigurationId, string? LastEtag, string Value);

    public record SetConfigurationResponse(ConfigurationId ConfigurationId, IList<ValidationFailure> Failures,
        string? Etag)
    {
        public bool Success => Failures.Count == 0;
    }
}