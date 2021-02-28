using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetConfigurationSectionHandler : IRequestHandler<GetConfigurationSectionCommand, ConfigurationSectionViewModel>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;
        public GetConfigurationSectionHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }

        public async Task<ConfigurationSectionViewModel> Handle(GetConfigurationSectionCommand request, CancellationToken cancellationToken)
        {
            var realm = await configinator.Realms.ByName(request.RealmName);
            return realm
                .GetConfigurationSection(request.ConfigurationSectionName)
                .ToConfigurationSectionViewModel(realm.Name);
        }
    }
}