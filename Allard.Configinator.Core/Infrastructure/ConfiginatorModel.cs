using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.Infrastructure
{
    public record GetValueRequest(ConfigurationId ConfigurationId, ConfigValueFormat Format);

    public enum ConfigValueFormat
    {
        Raw,
        Resolved,
    }

    // todo: return actual value
    public record GetConfigurationResponse(
        ConfigurationId ConfigurationId,
        bool Exists,
        JsonDocument ResolvedValue,
        List<MergedProperty> PropertyDetail)
    {
        public bool Existing => PropertyDetail.Count > 0;
    }

    public record SetConfigurationRequest(
        ConfigurationId ConfigurationId, 
        ConfigValueFormat format,
        JsonDocument Value);

    public record SetConfigurationResponse(
        ConfigurationId ConfigurationId, 
        IList<ValidationFailure> Failures)
    {
        public bool Success => Failures.Count == 0;
    }
}