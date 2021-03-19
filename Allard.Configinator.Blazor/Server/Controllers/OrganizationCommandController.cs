using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Blazor.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/organizations")]
    public class OrganizationCommandController : Controller
    {
        private readonly IOrganizationRepository repo;

        public OrganizationCommandController(IOrganizationRepository repo)
        {
            this.repo = repo;
        }
        
        [HttpPost]
        public async Task<CreateOrganizationResponse> CreateOrganization(CreateOrganizationRequest request)
        {
            var organization = new OrganizationAggregate(new OrganizationId(request.OrganizationId));
            await repo.SaveAsync(organization);
            return new CreateOrganizationResponse(request.OrganizationId);
        }
    }
}