using System.ComponentModel.DataAnnotations;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class CreateOrganizationRequest
    {
        [StringLength(20, MinimumLength = 5)]        
        [RegularExpression(Constants.NameRegex, ErrorMessage = "The OrganizationId must be lower case letters and dashes.")]
        public string OrganizationId { get; set; }
    }
}