using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels;
using Allard.Configinator.Blazor.Shared.ViewModels.Configuration;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Blazor.Server.Commands
{
    public record GetValueExplainedCommand(
        ConfigurationId ConfigurationId) : IRequest<ObjectViewModel>;

    public class GetConfigurationValueExplainedHandler
        : IRequestHandler<GetValueExplainedCommand, ObjectViewModel>
    {
        private readonly IMediator mediator;

        public GetConfigurationValueExplainedHandler(
            IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<ObjectViewModel> Handle(
            GetValueExplainedCommand request,
            CancellationToken cancellationToken)
        {
            var resolvedRequest = new GetValueCommand(request.ConfigurationId);
            var resolved = await mediator.Send(resolvedRequest, cancellationToken);
            return ToViewModel(resolved.Object);
        }

        private static ObjectViewModel ToViewModel(ObjectValue value)
        {
            var properties = value
                .Properties
                .Select(p => new PropertyValueViewModel
                {
                    Layers = p.Layers
                        .Select(l => new PropertyValueLayerViewModel(l.LayerName, l.Transition.ToString(), l.Value))
                        .ToList(),
                    Name = p.Name,
                    ObjectPath = p.ObjectPath,
                    Value = p.Value
                })
                .ToList();

            var objects = value.Objects.Select(ToViewModel).ToList();
            return new ObjectViewModel(value.ObjectPath, value.Name, properties, objects);
        }
    }
}