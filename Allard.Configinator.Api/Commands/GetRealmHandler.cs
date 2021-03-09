using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetRealmHandler : IRequestHandler<GetRealmCommand, RealmViewModel>
    {
        private readonly IConfiginatorService configinatorService;

        public GetRealmHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<RealmViewModel> Handle(GetRealmCommand request, CancellationToken cancellationToken)
        {
            return (await configinatorService.GetOrganizationByNameAsync(request.OrganizationName))
                .GetRealmByName(request.RealmName)
                .ToViewModel();
        }
    }
}