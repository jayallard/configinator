using Allard.Configinator.Core.Infrastructure;

namespace Allard.Configinator.Core.DocumentValidator
{
    public record ValidationFailure(ConfigurationId ConfigurationId, string Path, string Code, string Message);
}