namespace Allard.Configinator.Core.DocumentValidator
{
    public record SchemaValidationFailure(string Path, string Code, string Message);
}