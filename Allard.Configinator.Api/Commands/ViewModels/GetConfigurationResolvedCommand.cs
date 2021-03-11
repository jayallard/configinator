using Allard.Configinator.Core.Infrastructure;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetConfigurationResolvededCommand
    (
        string OrganizationName,
        string HabitatName,
        string RealmName,
        string ConfigurationSectionName) : IRequest<ConfigurationValue>;
}