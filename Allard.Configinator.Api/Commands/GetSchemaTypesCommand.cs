using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetSchemaTypesCommand(string OrganizationName) : IRequest<SchemaTypesViewModel>;
    
    public class GetSchemaTypesHandler : IRequestHandler<GetSchemaTypesCommand, SchemaTypesViewModel>
    {
        private readonly IConfiginatorService configinatorService;

        public GetSchemaTypesHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<SchemaTypesViewModel> Handle(GetSchemaTypesCommand request,
            CancellationToken cancellationToken)
        {
            return (await configinatorService
                    .GetOrganizationByNameAsync(request.OrganizationName))
                .SchemaTypes
                .ToViewModel();
        }
    }
}