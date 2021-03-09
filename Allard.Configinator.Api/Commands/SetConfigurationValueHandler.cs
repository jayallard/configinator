using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class SetConfigurationValueHandler : IRequestHandler<SetConfigurationValueCommand, Blah>
    {
        private readonly IConfiginatorService configinatorService;

        public SetConfigurationValueHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<Blah> Handle(SetConfigurationValueCommand request, CancellationToken cancellationToken)
        {
            var configinator = await configinatorService.GetConfiginatorByNameAsync(request.OrganizationName);
            var id = new ConfigurationId(
                request.OrganizationName,
                request.HabitatName,
                request.RealmName,
                request.ConfigurationSectionName);
            var setRequest = new SetConfigurationRequest(id, request.PreviousEtag, request.Value);
            var response = await configinator.SetValueAsync(setRequest);

            // todo: convert response
            return new Blah();
        }
    }

    public class Blah
    {
    }
}