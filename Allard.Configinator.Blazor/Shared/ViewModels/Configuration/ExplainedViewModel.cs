using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Configuration
{
    public record ObjectViewModel(string ObjectPath, string Name, List<PropertyValueViewModel> Properties,
        List<ObjectViewModel> Objects);

    public class PropertyValueViewModel
    {
        public string ObjectPath { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public List<PropertyValueLayerViewModel> Layers { get; set; }
    }

    public record PropertyValueLayerViewModel(string Name, string Transition, object Value);
}