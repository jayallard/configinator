using System.Collections.Generic;
using System.Text.Json.Serialization;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class PropertyViewModel
    {
        public string Name { get; init; }
        public bool IsRequired { get; init; }
        public SchemaTypeId SchemaTypeId { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<PropertyViewModel> Properties { get; set; }
    }
}