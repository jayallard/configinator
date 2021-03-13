using System.Collections.Generic;
using Allard.Configinator.Core.DocumentMerger;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record ExplainedViewModel(List<ExplainedProperty> Properties);
    public record ExplainedProperty(string Path, string Name, object Value, List<ExplainedPropertyLayer> Layers);
    public record ExplainedPropertyLayer(string Name, Transition Transition, object Value);
}