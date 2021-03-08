namespace Allard.Configinator.Core.DocumentValidator
{
    public record ValidationFailure(string Path, string Code, string Message);
}