using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Schema;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetSchemaTypeHandler : IRequestHandler<GetSchemaTypeCommand, SchemaTypeViewModel>
    {
        private readonly ISchemaService schemaService;

        public GetSchemaTypeHandler(ISchemaService schemaService)
        {
            this.schemaService = schemaService;
        }

        public async Task<SchemaTypeViewModel> Handle(GetSchemaTypeCommand request, CancellationToken cancellationToken)
        {
            return (await schemaService.GetSchemaTypeAsync(request.SchemaTypeId))
                .ToSchemaTypeViewModel();
        }
    }
}