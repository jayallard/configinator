namespace Allard.Configinator.Api.Controllers.ViewModels
{
    public record SetValueRequestModel(string? PreviousETag, string Value);
}