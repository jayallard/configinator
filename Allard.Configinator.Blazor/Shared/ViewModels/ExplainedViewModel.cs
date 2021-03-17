using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public record ObjectViewModel(string Path, string Name, List<PropertyViewModel> Properties,
        List<ObjectViewModel> Objects);
    
    public class PropertyViewModel
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public List<PropertyLayerViewModel> Layers { get; set; } 
    }

    public record PropertyLayerViewModel(string Name, string Transition, object Value);
}