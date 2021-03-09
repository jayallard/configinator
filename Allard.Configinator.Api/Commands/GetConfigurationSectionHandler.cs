using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class
        GetConfigurationSectionHandler : IRequestHandler<GetConfigurationSectionCommand, ConfigurationSectionViewModel
        >
    {
        private readonly IConfiginatorService configinatorService;

        public GetConfigurationSectionHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<ConfigurationSectionViewModel> Handle(GetConfigurationSectionCommand request,
            CancellationToken cancellationToken)
        {
            var configinator = await configinatorService.GetConfiginatorByNameAsync(request.OrganizationName);
            var realm = configinator.Organization.GetRealmByName(request.RealmName);
            var cs = realm.GetConfigurationSection(request.ConfigurationSectionName);
            return new ConfigurationSectionViewModel
            {
                ConfigurationSectionId = cs.ConfigurationSectionId,
                Path = cs.Path,
                RealmId = realm.RealmId,
                SchemaTypeId = cs.SchemaTypeId.FullId
            };
        }
    }
}