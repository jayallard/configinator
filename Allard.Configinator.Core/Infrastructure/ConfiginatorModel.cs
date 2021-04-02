using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.Infrastructure
{
    public record GetValueRequest(ConfigurationId ConfigurationId, string ValuePath = null);

    public record GetValueResponse(
        ConfigurationId ConfigurationId,
        bool Exists,
        JsonDocument Value);

    public record SetValueRequest(
        ConfigurationId ConfigurationId,
        string SettingsPath,
        JsonDocument Value);

    public record SetValueResponse(
        ConfigurationId ConfigurationId,
        IList<ValidationFailure> Failures)
    {
        public bool Success => Failures.Count == 0;
    }
}