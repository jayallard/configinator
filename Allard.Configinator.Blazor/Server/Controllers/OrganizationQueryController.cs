using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Server.Commands;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Blazor.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/organizations")]
    public class OrganizationQueryController : Controller
    {
        private readonly IMediator mediator;
        private readonly IOrganizationQueries queries;
        
        public OrganizationQueryController(IMediator mediator, IOrganizationQueries queries)
        {
            this.mediator = mediator;
            this.queries = queries;
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
        
        [HttpGet]
        public async Task<IEnumerable<OrganizationId>> GetOrganizations(string organizationId)
        {
            return await queries.GetOrganizationIds();
        }
    }
}