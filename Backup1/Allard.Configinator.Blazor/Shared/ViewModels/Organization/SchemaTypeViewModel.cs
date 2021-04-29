using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class SchemaTypeViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [RegularExpression(Constants.NameRegex,
            ErrorMessage = "The SchemaTypeId must be lower case letters and dashes.")]
        public string SchemaTypeId { get; set; }

        public IEnumerable<PropertyViewModel> Properties { get; set; }
        public List<Link> Links { get; set; }
    }
}