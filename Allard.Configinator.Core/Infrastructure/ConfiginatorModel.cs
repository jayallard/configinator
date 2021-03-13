using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.Infrastructure
{
    public record GetValueRequest(ConfigurationId ConfigurationId, ValueFormat Format);

    /// <summary>
    ///     The ways of getting and saving values.
    /// </summary>
    public enum ValueFormat
    {
        /// <summary>
        ///     The exact string, as stored.
        ///     NOTE: not sure the implication of VARIABLES on this.
        ///     If variables are resolved, that wouldn't be RAW.
        /// </summary>
        Raw,

        /// <summary>
        ///     Work with the resolved value.
        /// </summary>
        Resolved
    }

    public record GetConfigurationResponse(
        ConfigurationId ConfigurationId,
        bool Exists,
        JsonDocument Value,
        List<MergedProperty> PropertyDetail)
    {
        public bool Existing => PropertyDetail.Count > 0;
    }

    public record SetConfigurationRequest(
        ConfigurationId ConfigurationId,
        ValueFormat Format,
        JsonDocument Value);

    public record SetConfigurationResponse(
        ConfigurationId ConfigurationId,
        IList<ValidationFailure> Failures)
    {
        public bool Success => Failures.Count == 0;
    }
}