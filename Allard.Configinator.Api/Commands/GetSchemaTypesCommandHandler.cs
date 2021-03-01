using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Schema;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    namespace Allard.Configinator.Api.Commands
    {
        public class GetSchemaTypesHandler : IRequestHandler<GetSchemaTypesCommand, SchemaTypesViewModel>
        {
            private readonly LinkHelper linkHelper;
            private readonly ISchemaService schemaService;

            public GetSchemaTypesHandler(ISchemaService schemaService, LinkHelper linkHelper)
            {
                this.schemaService = schemaService;
                this.linkHelper = linkHelper;
            }

            public async Task<SchemaTypesViewModel> Handle(GetSchemaTypesCommand request,
                CancellationToken cancellationToken)
            {
                return new(
                    (await schemaService.GetSchemaTypesAsync()).Select(t => t.ToSchemaTypeViewModel()),
                    linkHelper
                        .CreateBuilder()
                        .AddRoot()
                        .AddSchemaTypes(true)
                        .Build()
                );
            }
        }
    }
}