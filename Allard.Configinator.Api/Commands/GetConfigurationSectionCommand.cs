using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetConfigurationSectionCommand(
        string OrganizationName,
        string RealmName,
        string ConfigurationSectionName) : IRequest<ConfigurationSectionViewModel>;
}