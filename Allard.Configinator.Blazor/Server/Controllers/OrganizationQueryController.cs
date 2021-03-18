using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Blazor.Server.Commands;
using Allard.Configinator.Blazor.Shared.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Blazor.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/organizations")]
    public class OrganizationQueryController : Controller
    {
        private readonly IMediator mediator;

        public OrganizationQueryController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        // [HttpGet]
        // [Route("{organizationId}/{realmId}")]
        // public async Task<RealmViewModel> GetRealm(string organizationId, string realmId)
        // {
        //     return await mediator.Send(new GetRealmCommand(organizationId, realmId));
        // }
        
        [HttpGet]
        [Route("{organizationId}")]
        public async Task<OrganizationViewModel> GetRealms(string organizationId)
        {
            return await mediator.Send(new GetOrganizationCommand(organizationId));
        }
    }
}