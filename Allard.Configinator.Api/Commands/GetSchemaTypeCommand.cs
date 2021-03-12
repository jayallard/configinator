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
        public GetSchemaTypeCommand(string organizationName, string schemaTypeId)
        {
            SchemaTypeId = HttpUtility.UrlDecode(schemaTypeId);
            OrganizationName = organizationName;
        }

        public string SchemaTypeId { get; }
        public string OrganizationName { get; }
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
            return (await configinatorService.GetOrganizationByNameAsync(request.OrganizationName))
                .GetSchemaType(SchemaTypeId.Parse(request.SchemaTypeId))
                .ToViewModel();
        }
    }
}