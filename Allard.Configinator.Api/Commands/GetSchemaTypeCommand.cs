using System.Web;
using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetSchemaTypeCommand : IRequest<SchemaTypeViewModel>
    {
        public string TypeId { get; }
        public GetSchemaTypeCommand(string typeId)
        {
            TypeId = HttpUtility.UrlDecode(typeId);
        }
    }
}