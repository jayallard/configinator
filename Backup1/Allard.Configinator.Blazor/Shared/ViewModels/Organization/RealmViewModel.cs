using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class RealmViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(Constants.NameRegex, ErrorMessage = "The RealmId must be lower case letters and dashes.")]
        public string RealmId { get; set; }

        public List<ConfigurationSectionViewModel> ConfigurationSections { get; set; } = new();
        public List<Link> Links { get; set; }
        public List<HabitatViewModel> Habitats { get; set; } = new();

        public HabitatViewModel GetHabitat(string habitatId)
        {
            return Habitats.Single(h => h.HabitatId == habitatId);
        }
    }
}