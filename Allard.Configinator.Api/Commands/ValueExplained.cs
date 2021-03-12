using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetConfigurationExplainedCommand(ConfigurationId ConfigurationId) : IRequest<ExplainedViewModel>;

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
            var resolvedRequest = new GetValueCommand(request.ConfigurationId, ConfigValueFormat.Resolved);
            var resolved = await mediator.Send(resolvedRequest, cancellationToken);
            if (!resolved.Exists)
            {
                return new ExplainedViewModel(new List<ExplainedProperty>());
            }
            
            var properties = resolved
                .Properties
                .Select(p => new ExplainedProperty(
                    p.Path,
                    p.Property.Name,
                    p.Property.Value,
                    p.Property.Layers.Select(l => new ExplainedPropertyLayer(
                            l.LayerName,
                            l.Transition,
                            l.Value))
                        .ToList()));
            return new ExplainedViewModel(properties.ToList());
        }
    }
}