using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetSchemaTypesCommand : IRequest<SchemaTypesViewModel>;
}