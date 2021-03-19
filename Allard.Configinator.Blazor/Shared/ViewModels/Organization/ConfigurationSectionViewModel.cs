using System.Collections.Generic;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class ConfigurationSectionViewModel
    {
        public OrganizationId OrganizationId { get; set; }
        public RealmId RealmId { get; set; }
        public SectionId SectionId { get; set; }
        public string Path { get; set; }
        public List<PropertyViewModel> Properties { get; set; }
        public List<Link> Links { get; set; }
    }
}