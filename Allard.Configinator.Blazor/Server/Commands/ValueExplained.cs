using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Blazor.Shared.ViewModels;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Blazor.Server.Commands
{
    public record GetConfigurationExplainedCommand(
        ConfigurationId ConfigurationId) : IRequest<ExplainedViewModel>;

    public class GetConfigurationValueExplainedHandler
        : IRequestHandler<GetConfigurationExplainedCommand, ExplainedViewModel>
    {
        private readonly IMediator mediator;

        public GetConfigurationValueExplainedHandler(
            IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<ExplainedViewModel> Handle(
            GetConfigurationExplainedCommand request,
            CancellationToken cancellationToken)
        {
            var resolvedRequest = new GetValueCommand(request.ConfigurationId, ValueFormat.Resolved);
            var resolved = await mediator.Send(resolvedRequest, cancellationToken);

            var props = resolved
                .Properties
                .Select(ToViewModel);
            return new ExplainedViewModel(props.ToList());
        }

        private ExplainedProperty ToViewModel(MergedProperty input)
        {
            // todo: layers dto
            var layers = input
                .Property
                .Layers
                .Select(l => new ExplainedPropertyLayer(l.LayerName, l.Transition.ToString(), l.Value))
                .ToList();

            var children = input
                .Children
                .Select(ToViewModel)
                .ToList();

            var output = new ExplainedProperty
            {
                Path = input.Path,
                Name = input.Property.Name,
                Value = (string)input.Property.Value ?? string.Empty,
                //OriginalValue = (string)input.Property.Value ?? string.Empty,
                Layers = layers,
                Children = children
            };
            return output;
        }
    }
}