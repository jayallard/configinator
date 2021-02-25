namespace Allard.Configinator.Schema.Validator
{
    // todo: just enough to get it started. a richer model
    // will evolve.
    public record TypeValidationError(string ValidatorType, string Path, string Error);
}