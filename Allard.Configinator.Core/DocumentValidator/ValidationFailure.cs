namespace Allard.Configinator.Core.DocumentValidator
{
    public record ValidationFailure(string HabitatId, string Path, string Code, string Message);
}