using System.Collections.Generic;

namespace Allard.Configinator.RazorPages.Pages
{
    public record ExplainedViewModel(List<ExplainedProperty> Properties);
    public record ExplainedProperty(string Path, string Name, object Value, string LastTransition, List<ExplainedPropertyLayer> Layers);
    public record ExplainedPropertyLayer(string Name, string Transition, object Value);
}