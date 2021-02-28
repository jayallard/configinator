using System.Linq;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Schema;

namespace Allard.Configinator.Api.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    namespace Allard.Configinator.Api.Commands
    {
        public class GetSchemaTypesHandler : IRequestHandler<GetSchemaTypesCommand, SchemaTypesViewModel>
        {
            private readonly ISchemaService schemaService;
            private readonly LinkHelper linkHelper;

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