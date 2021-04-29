using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class ConfigurationSectionViewModel
    {
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(Constants.NameRegex,
            ErrorMessage = "The OrganizationId must be lower case letters and dashes.")]
        public string OrganizationId { get; set; }

        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(Constants.NameRegex, ErrorMessage = "The RealmId must be lower case letters and dashes.")]
        public string RealmId { get; set; }

        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(Constants.NameRegex, ErrorMessage = "The SectionId must be lower case letters and dashes.")]
        public string SectionId { get; set; }

        public List<PropertyViewModel> Properties { get; set; } = new();
        public List<Link> Links { get; set; }
    }
}