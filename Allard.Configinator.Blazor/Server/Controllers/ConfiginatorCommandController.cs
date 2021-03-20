using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Server.Commands;
using Allard.Configinator.Core.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Blazor.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/configuration")]
    public class ConfiginatorCommandController : Controller
    {
        private readonly IMediator mediator;

        public ConfiginatorCommandController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        ///     Sets the value for the habitat
        ///     This may be a full document including values from
        ///     the bases. This will be crunched down to only the
        ///     values that are different from the bases.
        ///     IE: (not done)
        ///     If the base has the value x.password=y,
        ///     and the posted document contains x.password=y,
        ///     then the value will not be saved to the habitat.
        ///     to force the save, use value-raw instead.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="realmId"></param>
        /// <param name="sectionId"></param>
        /// <param name="habitatId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{organizationId}/{realmId}/{sectionId}/{habitatId}/value-resolved")]
        public async Task<SetConfigurationResponse> SetConfigurationValueResolved(
            string organizationId,
            string realmId,
            string sectionId,
            string habitatId,
            [FromBody] JsonDocument value)
        {
            // todo: location header
            var id = new ConfigurationId(organizationId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new SetValueCommand(id, ValueFormat.Resolved, value));
            if (!response.Success) Response.StatusCode = (int) HttpStatusCode.BadRequest;
            return response;
        }
    }
}