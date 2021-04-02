using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core;
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
            await repo.CreateAsync(organization);
            return new CreateOrganizationResponse(request.OrganizationId);
        }

        [HttpPost]
        [Route("{organizationId}/realms")]
        public async Task AddRealmToOrganization(string organizationId, [FromBody] RealmViewModel realm)
        {
            organizationId.EnsureValue(nameof(organizationId));
            var org = await repo.GetOrganizationByIdAsync(organizationId);
            var r = org.AddRealm(realm.RealmId);

            // habitats
            foreach (var h in realm.Habitats) r.AddHabitat(h.HabitatId, h.BaseHabitatId);

            // config sections
            foreach (var c in realm.ConfigurationSections)
            {
                var props = c
                    .Properties
                    .Select(p =>
                        new SchemaTypeProperty(p.Name, SchemaTypeId.Parse(p.SchemaTypeId), p.IsSecret, !p.IsRequired))
                    .ToList()
                    .AsReadOnly();
                r.AddConfigurationSection(c.SectionId, props, "");
            }

            await repo.UpdateAsync(org);
        }
    }
}