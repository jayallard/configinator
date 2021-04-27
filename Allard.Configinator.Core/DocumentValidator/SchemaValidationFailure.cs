using Allard.Configinator.Core.Infrastructure;

namespace Allard.Configinator.Core.DocumentValidator
{
    public record SchemaValidationFailure(string Path, string Code, string Message);

    public record ConfiginatorValidationFailure(ConfigurationId ConfigurationId, string Path, string Code,
        string Message);
}