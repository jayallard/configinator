using System.Collections.Generic;
using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public static class GetRealms
    {
    }

    public record GetRealmsCommand() : IRequest<IEnumerable<RealmViewModel>>;
}