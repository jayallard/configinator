using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Blazor.Server.Commands
{
    public record SetValueCommand(
        ConfigurationId ConfigurationId,
        JsonDocument Value) : IRequest<SetValueResponse>;

    public record GetValueCommand(
        ConfigurationId ConfigurationId) : IRequest<ConfigurationValue>;

    public class GetValueHandler : IRequestHandler<GetValueCommand, ConfigurationValue>
    {
        private readonly IConfiginatorService configinatorService;

        public GetValueHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<ConfigurationValue> Handle(
            GetValueCommand request,
            CancellationToken cancellationToken)
        {
            var get = new GetValueRequest(request.ConfigurationId);
            var configinator =
                await configinatorService.GetConfiginatorByIdAsync(request.ConfigurationId.OrganizationId);
            var result = await configinator.GetValueAsync(get);
            return new ConfigurationValue(request.ConfigurationId, result.Exists, result.Value,
                result.ObjectValue);
        }
    }

    public class SetValueHandler : IRequestHandler<SetValueCommand, SetValueResponse>
    {
        private readonly IConfiginatorService configinatorService;

        public SetValueHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<SetValueResponse> Handle(SetValueCommand request,
            CancellationToken cancellationToken)
        {
            var configinator =
                await configinatorService.GetConfiginatorByIdAsync(request.ConfigurationId.OrganizationId);
            var setRequest = new SetValueRequest(request.ConfigurationId, "TODO", request.Value);
            var response = await configinator.SetValueAsync(setRequest);

            // todo: map failures to dto
            return new SetValueResponse(response.ConfigurationId, response.Failures);
        }
    }
}