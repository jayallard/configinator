using System.Threading.Tasks;
using Allard.Configinator.Blazor.Server.Commands;
using Allard.Configinator.Blazor.Shared;
using Allard.Configinator.Blazor.Shared.ViewModels.Configuration;
using Allard.Configinator.Core.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Blazor.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/configuration")]
    public class ConfiginatorQueriesController : Controller
    {
        private readonly LinkHelper links;
        private readonly IMediator mediator;

        public ConfiginatorQueriesController(IMediator mediator, LinkHelper links)
        {
            this.mediator = mediator;
            this.links = links;
        }

        [HttpGet]
        [Route("{organizationId}/{realmId}/{sectionId}/{habitatId}/value-explained")]
        public async Task<ObjectViewModel> GetConfigurationValueExplained(
            string organizationId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            var id = new ConfigurationId(organizationId, realmId, sectionId, habitatId);
            var result = await mediator.Send(new GetConfigurationExplainedCommand(id));
            return result;
        }
    }
}