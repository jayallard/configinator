using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class PropertyViewModel
    {
        public string Name { get; init; }
        public bool IsRequired { get; init; }
        public string TypeId { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<PropertyViewModel> Properties { get; set; }
    }
}