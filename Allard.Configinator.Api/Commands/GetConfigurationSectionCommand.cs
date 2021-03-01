using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetConfigurationSectionCommand : IRequest<ConfigurationSectionViewModel>
    {
        public GetConfigurationSectionCommand(string realmName, string configurationSectionName)
        {
            RealmName = realmName;
            ConfigurationSectionName = configurationSectionName;
        }

        public string RealmName { get; }
        public string ConfigurationSectionName { get; }
    }
}