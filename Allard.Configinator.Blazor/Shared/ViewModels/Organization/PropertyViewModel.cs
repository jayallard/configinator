using System.ComponentModel.DataAnnotations;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class PropertyViewModel
    {
        [StringLength(20, MinimumLength = 5)]        
        [RegularExpression(Constants.NameRegex, ErrorMessage = "The OrganizationId must be lower case letters and dashes.")]
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSecret { get; set; }
        public string SchemaTypeId { get; set; }
    }
}