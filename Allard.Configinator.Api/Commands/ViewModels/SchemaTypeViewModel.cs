using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class SchemaTypeViewModel
    {
        public string TypeId { get; set; }
        
        public IEnumerable<PropertyViewModel> Properties { get; set; }
        public IEnumerable<Link> Links { get; set; } 
    }
}