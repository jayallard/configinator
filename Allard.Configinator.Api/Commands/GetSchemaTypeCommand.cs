using System.Web;
using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record GetSchemaTypeCommand : IRequest<SchemaTypeViewModel>
    {
        public GetSchemaTypeCommand(string typeId)
        {
            SchemaTypeId = HttpUtility.UrlDecode(typeId);
        }

        public string SchemaTypeId { get; }
    }
}