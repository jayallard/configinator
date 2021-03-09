using System.Collections.Generic;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class SchemaTypeViewModel
    {
        public SchemaTypeId SchemaTypeId { get; set; }

        public IEnumerable<PropertyViewModel> Properties { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}