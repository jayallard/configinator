using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers.ViewModels;
using Allard.Configinator.Core.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Api.Controllers
{
    [ApiController]
    [Route("/api/v1")]
    public class ConfiginatorQueriesController : Controller
    {
        private readonly LinkHelper links;
        private readonly IMediator mediator;

        public ConfiginatorQueriesController(IMediator mediator, LinkHelper links)
        {
            this.mediator = mediator;
            this.links = links;
        }

        // ========================================================
        // ========================================================
        // ========================================================
        // ========================================================
        // todo


        /*
        [HttpPut]
        [Route("realms/{realmName}/sections/{configurationSectionName}/value-resolved/{habitat}")]
        public async Task<Blah> SetConfigurationValueResolved(
            string realmName,
            string configurationSectionName,
            string habitat,
            [FromBody] SetValueRequestModel value)
        {
            return await mediator.Send(
                new SetConfigurationRawCommand
                {
                    HabitatName = habitat,
                    RealmName = realmName,
                    ConfigurationSectionName = configurationSectionName,
                    Value = value.Value
                });
        }*/


        public string OrganizationId { get; } = "allard";

        /// <summary>
        ///     Get the raw value stored for the habitat.
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="realmId"></param>
        /// <param name="sectionId"></param>
        /// <param name="habitatId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-raw")]
        public async Task<JsonDocument?> GetConfigurationValueRaw(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new GetValueCommand(id, ValueFormat.Raw));
            if (response.Exists) return response.ResolvedValue;
            Response.StatusCode = 404;
            return null;
        }

        /// <summary>
        ///     Set the raw value for the habitat.
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="realmId"></param>
        /// <param name="sectionId"></param>
        /// <param name="habitatId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-raw")]
        public async Task<SetConfigurationResponse> SetConfigurationValueRaw(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId,
            [FromBody] JsonDocument value)
        {
            // todo: location header
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new SetValueCommand(id, ValueFormat.Raw, value));
            if (!response.Success) Response.StatusCode = (int) HttpStatusCode.BadRequest;

            return response;
        }

        /// <summary>
        ///     Get the resolved value for the habitat.
        ///     The values of the base habitats are merged into the
        ///     raw value of the requested habittat.
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="realmId"></param>
        /// <param name="sectionId"></param>
        /// <param name="habitatId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-resolved")]
        public async Task<JsonDocument> GetConfigurationValueResolved(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new GetValueCommand(id, ValueFormat.Resolved));
            return response.ResolvedValue;
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
        /// <param name="orgId"></param>
        /// <param name="realmId"></param>
        /// <param name="sectionId"></param>
        /// <param name="habitatId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-resolved")]
        public async Task<SetConfigurationResponse> SetConfigurationValueResolved(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId,
            [FromBody] JsonDocument value)
        {
            // todo: location header
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new SetValueCommand(id, ValueFormat.Resolved, value));
            if (!response.Success) Response.StatusCode = (int) HttpStatusCode.BadRequest;

            return response;
        }

        [HttpGet]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-explained")]
        public async Task<ExplainedViewModel> GetConfigurationValueExplained(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            return await mediator.Send(new GetConfigurationExplainedCommand(id));
        }

        [HttpGet]
        public RootViewModel GetRoot()
        {
            return new()
            {
                Links = links
                    .CreateBuilder()
                    .AddSchemaTypes()
                    .AddRealms()
                    .AddRoot(true)
                    .Build()
            };
        }

        [HttpGet]
        [Route("schemaTypes")]
        public async Task<SchemaTypesViewModel> GetSchemaTypes()
        {
            return await mediator.Send(new GetSchemaTypesCommand(OrganizationId));
        }

        [HttpGet]
        [Route("schemaTypes/{typeId}")]
        public async Task<SchemaTypeViewModel> GetSchemaType(string schemaTypeId)
        {
            return await mediator.Send(new GetSchemaTypeCommand(OrganizationId, schemaTypeId));
        }

        [HttpGet]
        [Route("{organizationId}/realms")]
        public async Task<RealmsViewModel> GetRealms(string organizationid)
        {
            return await mediator.Send(new GetRealmsCommand(organizationid));
        }

        [HttpGet]
        [Route("realms/{realmName}")]
        public async Task<RealmViewModel> GetRealm(string realmName)
        {
            return await mediator.Send(new GetRealmCommand(OrganizationId, realmName));
        }


        [HttpGet]
        [Route("realms/{realmName}/sections/{configurationSectionName}")]
        public async Task<ConfigurationSectionViewModel> GetConfigurationSection(
            string realmName,
            string configurationSectionName)
        {
            return await mediator.Send(
                new GetConfigurationSectionCommand(OrganizationId, realmName, configurationSectionName));
        }
    }
}