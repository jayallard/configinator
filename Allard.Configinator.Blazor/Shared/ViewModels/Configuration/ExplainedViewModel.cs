using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels
{
    public record ObjectViewModel(string Path, string Name, List<PropertyValueViewModel> Properties,
        List<ObjectViewModel> Objects);

    public class PropertyValueViewModel
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public List<PropertyValueLayerViewModel> Layers { get; set; }
    }

    public record PropertyValueLayerViewModel(string Name, string Transition, object Value);
}