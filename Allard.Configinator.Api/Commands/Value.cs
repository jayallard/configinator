using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record SetValueCommand(
        ConfigurationId ConfigurationId,
        ValueFormat Format,
        JsonDocument Value) : IRequest<SetConfigurationResponse>;

    public record GetValueCommand(
        ConfigurationId ConfigurationId,
        ValueFormat Format) : IRequest<ConfigurationValue>;
    
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
            var get = new GetValueRequest(request.ConfigurationId, request.Format);
            var configinator =
                await configinatorService.GetConfiginatorByNameAsync(request.ConfigurationId.OrganizationId);
            var result = await configinator.GetValueAsync(get);
            return new ConfigurationValue(request.ConfigurationId, result.Exists, result.Value,
                result.PropertyDetail);
        }
    }

    public class SetValueHandler : IRequestHandler<SetValueCommand, SetConfigurationResponse>
    {
        private readonly IConfiginatorService configinatorService;

        public SetValueHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<SetConfigurationResponse> Handle(SetValueCommand request,
            CancellationToken cancellationToken)
        {
            var configinator =
                await configinatorService.GetConfiginatorByNameAsync(request.ConfigurationId.OrganizationId);
            var setRequest = new SetConfigurationRequest(request.ConfigurationId, request.Format, request.Value);
            var response = await configinator.SetValueAsync(setRequest);

            // todo: map failures to dto
            return new SetConfigurationResponse(response.ConfigurationId, response.Failures);
        }
    }
}