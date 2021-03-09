using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetSchemaTypesCommand(string OrganizationName) : IRequest<SchemaTypesViewModel>;
}