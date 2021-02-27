using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class PropertyViewModel
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public string TypeId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<PropertyViewModel> Properties { get; set; }
    }
}