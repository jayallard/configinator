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
        ConfigurationId ConfigurationId) : IRequest<ObjectViewModel>;

    public class GetConfigurationValueExplainedHandler
        : IRequestHandler<GetConfigurationExplainedCommand, ObjectViewModel>
    {
        private readonly IMediator mediator;

        public GetConfigurationValueExplainedHandler(
            IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<ObjectViewModel> Handle(
            GetConfigurationExplainedCommand request,
            CancellationToken cancellationToken)
        {
            var resolvedRequest = new GetValueCommand(request.ConfigurationId, ValueFormat.Resolved);
            var resolved = await mediator.Send(resolvedRequest, cancellationToken);
            return ToViewModel(resolved.Object);
        }

        private static ObjectViewModel ToViewModel(ObjectValue value)
        {
            var properties = value
                .Properties
                .Select(p => new PropertyViewModel
                {
                    Layers = p.Layers
                        .Select(l => new PropertyLayerViewModel(l.LayerName, l.Transition.ToString(), l.Value))
                        .ToList(),
                    Name = p.Name,
                    Path = p.Path,
                    Value = p.Value
                })
                .ToList();

            var objects = value.Objects.Select(ToViewModel).ToList();
            return new ObjectViewModel(value.Path, value.Name, properties, objects);
        }
    }
}