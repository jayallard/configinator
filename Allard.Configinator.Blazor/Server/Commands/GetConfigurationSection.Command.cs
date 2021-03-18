using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Blazor.Server.Commands
{
    public record GetConfigurationSectionCommand(
        string OrganizationId,
        string RealmId,
        string SectionId) : IRequest<ConfigurationSectionViewModel>;

    public class GetConfigurationSectionHandler
        : IRequestHandler<GetConfigurationSectionCommand, ConfigurationSectionViewModel>
    {
        private readonly IConfiginatorService configinatorService;

        public GetConfigurationSectionHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<ConfigurationSectionViewModel> Handle(GetConfigurationSectionCommand request,
            CancellationToken cancellationToken)
        {
            var configinator = await configinatorService.GetConfiginatorByIdAsync(request.OrganizationId);
            var realm = configinator.Organization.GetRealmByName(request.RealmId);
            var cs = realm.GetConfigurationSection(request.SectionId);
            return new ConfigurationSectionViewModel
            {
                SectionId = cs.SectionId,
                Path = cs.Path,
                RealmId = realm.RealmId,
                SchemaTypeId = cs.SchemaType.SchemaTypeId.FullId
            };
        }
    }
}