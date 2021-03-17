using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public record ExplainedViewModel(List<ExplainedProperty> Properties);

    public class ExplainedProperty
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public List<ExplainedPropertyLayer> Layers { get; set; } 
        public List<ExplainedProperty> Children { get; set; }   
    }

    public record ExplainedPropertyLayer(string Name, string Transition, object Value);
}