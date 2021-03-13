using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetSchemaTypeCommand : IRequest<SchemaTypeViewModel>
    {
        public GetSchemaTypeCommand(string organizationId, string schemaTypeId)
        {
            SchemaTypeId = HttpUtility.UrlDecode(schemaTypeId);
            OrganizationId = organizationId;
        }

        public string SchemaTypeId { get; }
        public string OrganizationId { get; }
    }

    public class GetSchemaTypeHandler : IRequestHandler<GetSchemaTypeCommand, SchemaTypeViewModel>
    {
        private readonly IConfiginatorService configinatorService;

        public GetSchemaTypeHandler(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        public async Task<SchemaTypeViewModel> Handle(GetSchemaTypeCommand request, CancellationToken cancellationToken)
        {
            return (await configinatorService.GetOrganizationByIdAsync(request.OrganizationId))
                .GetSchemaType(SchemaTypeId.Parse(request.SchemaTypeId))
                .ToViewModel();
        }
    }
}