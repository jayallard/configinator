using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record SetConfigurationResolvedCommand
        (ConfigurationId ConfigurationId, JsonDocument Value) : IRequest<SetConfigurationResponse>;

    public record GetConfigurationResolvedCommand(ConfigurationId ConfigurationId) : IRequest<ConfigurationValue>;

    public class
        GetConfigurationValueResolvedHandler : IRequestHandler<GetConfigurationResolvedCommand, ConfigurationValue>
    {
        private readonly IConfiginatorService configinatorService;

        public GetConfigurationValueResolvedHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<ConfigurationValue> Handle(
            GetConfigurationResolvedCommand request,
            CancellationToken cancellationToken)
        {
            var get = new GetConfigurationRequest(request.ConfigurationId);
            var configinator =
                await configinatorService.GetConfiginatorByNameAsync(request.ConfigurationId.OrganizationId);
            var result = await configinator.GetValueResolvedAsync(get);
            return new ConfigurationValue(request.ConfigurationId, result.Exists, result.ResolvedValue,
                result.PropertyDetail);
        }
    }

    public class SetConfigurationValueResolvedHandler
        : IRequestHandler<SetConfigurationResolvedCommand, SetConfigurationResponse>
    {
        private readonly IConfiginatorService configinatorService;

        public SetConfigurationValueResolvedHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<SetConfigurationResponse> Handle(SetConfigurationResolvedCommand request,
            CancellationToken cancellationToken)
        {
            // TODO: do a diff between this document and the merge of the upper docs.
            // whatever is left is what needs to be saved. 
            var configinator =
                await configinatorService.GetConfiginatorByNameAsync(request.ConfigurationId.OrganizationId);
            var setRequest = new SetConfigurationRequest(request.ConfigurationId, request.Value);
            var response = await configinator.SetValueRawAsync(setRequest);

            // todo: map failures to dto
            return new SetConfigurationResponse(response.ConfigurationId, response.Failures);
        }
    }
}