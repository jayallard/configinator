using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class SchemaTypeViewModel
    {
        public string SchemaTypeId { get; set; }

        public IEnumerable<PropertyViewModel> Properties { get; set; }
        public List<Link> Links { get; set; }
    }
}