using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class SchemaTypeViewModel
    {
        public string SchemaTypeId { get; set; }

        public IEnumerable<PropertyViewModel> Properties { get; set; }
        public List<Link> Links { get; set; }
    }
}