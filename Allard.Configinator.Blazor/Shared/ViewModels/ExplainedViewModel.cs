using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public record ExplainedViewModel(List<ExplainedProperty> Properties);

    public class ExplainedProperty
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string OriginalValue { get; set; }

        public bool IsValueChanged => !Equals(Value, OriginalValue);

        public List<ExplainedPropertyLayer> Layers { get; set; } 
        public List<ExplainedProperty> Children { get; set; }   
    }

    public record ExplainedPropertyLayer(string Name, string Transition, object Value);
}