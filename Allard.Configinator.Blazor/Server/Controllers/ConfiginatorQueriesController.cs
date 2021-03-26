using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Server.Commands;
using Allard.Configinator.Blazor.Shared.ViewModels.Configuration;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Blazor.Server.Controllers
{
    [ApiController]
    public class ConfiginatorQueriesController : Controller
    {
        private readonly IMediator mediator;
        private readonly IConfiginatorService service;

        public ConfiginatorQueriesController(IMediator mediator, IConfiginatorService service)
        {
            this.mediator = mediator;
            this.service = service;
        }

        [HttpGet]
        [Route("/api/v1/configuration-detail{organizationId}/{realmId}/{sectionId}/{habitatId}")]
        public async Task<ObjectViewModel> GetValueExplained(
            string organizationId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            // TODO: add {**settingPath}
            var id = new ConfigurationId(organizationId, realmId, sectionId, habitatId);
            var result = await mediator.Send(new GetConfigurationExplainedCommand(id));
            return result;
        }
        
        [HttpGet]
        [Route("/api/v1/configuration/{organizationId}/{realmId}/{sectionId}/{habitatId}/{**settingPath}")]
        public async Task<JsonDocument> GetValue(string organizationId,
            string realmId,
            string sectionId,
            string habitatId,
            string settingPath)
        {
            var configinator = await service.GetConfiginatorByIdAsync(organizationId);
            var configurationId = new ConfigurationId(organizationId, realmId, sectionId, habitatId);
            var request = new GetValueRequest(configurationId, ValueFormat.Resolved, settingPath);
            var response = await configinator.GetValueAsync(request);
            return response.Value;
        }
    }
}