using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public class PropertyViewModel
    {
        public string Name { get; init; }
        public bool IsRequired { get; init; }
        public string SchemaTypeId { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<PropertyViewModel> Properties { get; set; }
    }
}