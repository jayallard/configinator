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
        private readonly LinkHelper linkHelper;

        public GetSchemaTypeHandler(ISchemaService schemaService, LinkHelper linkHelper)
        {
            this.schemaService = schemaService;
            this.linkHelper = linkHelper;
        }

        public async Task<SchemaTypeViewModel> Handle(GetSchemaTypeCommand request, CancellationToken cancellationToken)
        {
            return (await schemaService.GetSchemaTypeAsync(request.TypeId)).ToSchemaTypeViewModel(linkHelper);
        }
    }
}