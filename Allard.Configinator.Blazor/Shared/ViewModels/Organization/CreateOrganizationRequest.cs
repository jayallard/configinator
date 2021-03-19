using System.ComponentModel.DataAnnotations;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class CreateOrganizationRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 5)]        
        [RegularExpression("[a-z-]+", ErrorMessage = "The id must be lower case letters and dashes.")]
        public string OrganizationId { get; set; }
    }
}