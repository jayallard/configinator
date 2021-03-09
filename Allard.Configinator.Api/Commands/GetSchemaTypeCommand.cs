using System.Web;
using Allard.Configinator.Api.Commands.ViewModels;
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
}