using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetConfigurationValueHandler : IRequestHandler<GetConfigurationValueCommand, ConfigurationValue>
    {
        private readonly IConfiginatorService configinatorService;

        public GetConfigurationValueHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<ConfigurationValue> Handle(GetConfigurationValueCommand request,
            CancellationToken cancellationToken)
        {
            var configinator = await configinatorService.GetConfiginatorByNameAsync(request.OrganizationName);
            var id = new ConfigurationId(
                request.OrganizationName,
                request.HabitatName,
                request.RealmName,
                request.ConfigurationSectionName);
            var get = new GetConfigurationRequest(id);
            var result = await configinator.GetValueAsync(get);
            return new ConfigurationValue(id, result.ETag, result.ResolvedValue);
        }
    }
}