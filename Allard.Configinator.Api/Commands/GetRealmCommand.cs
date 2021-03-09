using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetRealmCommand(
        string OrganizationName,
        string RealmName) : IRequest<RealmViewModel>;
}