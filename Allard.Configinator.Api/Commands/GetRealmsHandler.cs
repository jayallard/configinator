using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetRealmsHandler : IRequestHandler<GetRealmsCommand, RealmsViewModel>
    {
        private readonly IConfiginatorService configinatorService;

        public GetRealmsHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<RealmsViewModel> Handle(
            GetRealmsCommand request,
            CancellationToken cancellationToken)
        {
            return (await configinatorService.GetOrganizationByNameAsync(request.OrganizationName))
                .Realms
                .ToViewModel();
        }
    }
}