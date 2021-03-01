using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetConfigurationValueCommand
        (string Habitat, string Realm, string ConfigurationSection) : IRequest<ConfigurationValue>;
}