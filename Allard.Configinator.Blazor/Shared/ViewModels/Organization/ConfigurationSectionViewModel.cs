using System.Collections.Generic;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class ConfigurationSectionViewModel
    {
        public string OrganizationId { get; set; }
        public string RealmId { get; set; }
        public string SectionId { get; set; }
        public string Path { get; set; } = "/x/y/z";
        public List<PropertyViewModel> Properties { get; set; } = new();
        public List<Link> Links { get; set; }
    }
}