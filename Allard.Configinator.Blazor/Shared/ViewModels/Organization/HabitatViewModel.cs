using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class HabitatViewModel
    {
        [StringLength(20, MinimumLength = 5)]        
        [RegularExpression(Constants.NameRegex, ErrorMessage = "The HabitatId must be lower case letters and dashes.")]
        public string HabitatId { get; set; }
        public List<string> BaseHabitatIds { get; set; } = new();
    }
}