using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Blazor.Server.Commands
{
    public record GetConfigurationExplainedCommand(
        ConfigurationId ConfigurationId) : IRequest<ExplainedObject>;

    public class GetConfigurationValueExplainedHandler
        : IRequestHandler<GetConfigurationExplainedCommand, ExplainedObject>
    {
        private readonly IMediator mediator;

        public GetConfigurationValueExplainedHandler(
            IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<ExplainedObject> Handle(
            GetConfigurationExplainedCommand request,
            CancellationToken cancellationToken)
        {
            var resolvedRequest = new GetValueCommand(request.ConfigurationId, ValueFormat.Resolved);
            var resolved = await mediator.Send(resolvedRequest, cancellationToken);

            var props = resolved
                .Object
                .Properties
                .Select(ToViewModel);
            return new ExplainedObject("", "", props.ToList(), new List<ExplainedObject>());
        }

        private ExplainedProperty ToViewModel(PropertyValue input)
        {
            // todo: layers dto
            var layers = input
                .Layers
                .Select(l => new ExplainedPropertyLayer(l.LayerName, l.Transition.ToString(), l.Value))
                .ToList();
            
            var output = new ExplainedProperty
            {
                Path = input.Path,
                Name = input.Name,
                Value = input.Value ?? string.Empty,
                Layers = layers
            };
            return output;
        }
    }
}