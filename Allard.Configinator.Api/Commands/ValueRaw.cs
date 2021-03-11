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
    public record SetConfigurationRawCommand(ConfigurationId ConfigurationId, JsonDocument Value) : IRequest<SetConfigurationResponse>;
    public record GetConfigurationRawCommand(ConfigurationId ConfigurationId) : IRequest<ConfigurationValue>;
    public record GetConfigurationResponse(ConfigurationId ConfigurationId, List<ValidationFailure> errors);

    public class GetConfigurationValueRawHandler : IRequestHandler<GetConfigurationRawCommand, ConfigurationValue>
    {
        private readonly IConfiginatorService configinatorService;

        public GetConfigurationValueRawHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<ConfigurationValue> Handle(
            GetConfigurationRawCommand request,
            CancellationToken cancellationToken)
        {
            var get = new GetConfigurationRequest(request.ConfigurationId);
            var configinator = await configinatorService.GetConfiginatorByNameAsync(request.ConfigurationId.OrganizationId);
            var result = await configinator.GetValueRawAsync(get);
            return new ConfigurationValue(request.ConfigurationId, result.Exists, result.ResolvedValue, null);
        }
    }
    
    public class SetConfigurationValueRawHandler : IRequestHandler<SetConfigurationRawCommand, SetConfigurationResponse>
    {
        private readonly IConfiginatorService configinatorService;

        public SetConfigurationValueRawHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<SetConfigurationResponse> Handle(SetConfigurationRawCommand request, CancellationToken cancellationToken)
        {
            var configinator = await configinatorService.GetConfiginatorByNameAsync(request.ConfigurationId.OrganizationId);
            var setRequest = new SetConfigurationRequest(request.ConfigurationId, request.Value);
            var response = await configinator.SetValueAsync(setRequest);
            
            // todo: map failures to dto
            return new SetConfigurationResponse(response.ConfigurationId, response.Failures);
        }
    }
}