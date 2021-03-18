using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels;
using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetOrganizationCommand(string OrganizationId) : IRequest<OrganizationViewModel>;

    public class GetRealmsHandler : IRequestHandler<GetOrganizationCommand, OrganizationViewModel>
    {
        private readonly IConfiginatorService configinatorService;

        public GetRealmsHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<OrganizationViewModel> Handle(
            GetOrganizationCommand request,
            CancellationToken cancellationToken)
        {
            return (await configinatorService.GetOrganizationByIdAsync(request.OrganizationId))
                .ToViewModel();
        }
    }
}