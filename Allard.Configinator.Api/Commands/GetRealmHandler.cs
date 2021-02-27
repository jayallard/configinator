using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetRealmHandler : IRequestHandler<GetRealmCommand, RealmViewModel>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;

        public GetRealmHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }

        public async Task<RealmViewModel> Handle(GetRealmCommand request, CancellationToken cancellationToken)
        {
            return (await configinator.Realms.ByName(request.Name)).ToRealmViewModel(linkHelper);
        }
    }
}