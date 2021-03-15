using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record ExplainedViewModel(List<ExplainedProperty> Properties);

    public record ExplainedProperty(string Path, string Name, object Value, List<ExplainedPropertyLayer> Layers);

    public record ExplainedPropertyLayer(string Name, string Transition, object Value);
}