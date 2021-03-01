using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class
        GetConfigurationSectionHandler : IRequestHandler<GetConfigurationSectionCommand, ConfigurationSectionViewModel>
    {
        private readonly Configinator configinator;

        public GetConfigurationSectionHandler(Configinator configinator)
        {
            this.configinator = configinator;
        }

        public async Task<ConfigurationSectionViewModel> Handle(GetConfigurationSectionCommand request,
            CancellationToken cancellationToken)
        {
            var realm = await configinator.Realms.ByName(request.RealmName);
            return realm
                .GetConfigurationSection(request.ConfigurationSectionName)
                .ToConfigurationSectionViewModel(realm.Name);
        }
    }
}