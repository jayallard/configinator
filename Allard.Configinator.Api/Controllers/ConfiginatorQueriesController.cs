using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers.ViewModels;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Api.Controllers
{
    //[HateosFilter]
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

        [HttpGet]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-raw")]
        public async Task<JsonDocument?> GetConfigurationValueRaw(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new GetConfigurationRawCommand(id));
            if (response.Exists) return response.ResolvedValue;
            Response.StatusCode = 404;
            return null;

        }

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
            var response = await mediator.Send(new SetConfigurationRawCommand(id, value));
            if (!response.Success)
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }

            return response;
        }
        
        [HttpGet]
        [Route("config/{orgId}/{realmId}/{sectionId}/{habitatId}/value-resolved")]
        public async Task<JsonDocument> GetConfigurationValueResolved(
            string orgId,
            string realmId,
            string sectionId,
            string habitatId)
        {
            var id = new ConfigurationId(orgId, realmId, sectionId, habitatId);
            var response = await mediator.Send(new GetConfigurationResolvedCommand(id));
            return response.ResolvedValue;

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


        public string OrganizationName { get; } = "allard";

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
            return await mediator.Send(new GetSchemaTypesCommand(OrganizationName));
        }

        [HttpGet]
        [Route("schemaTypes/{typeId}")]
        public async Task<SchemaTypeViewModel> GetSchemaType(string schemaTypeId)
        {
            return await mediator.Send(new GetSchemaTypeCommand(OrganizationName, schemaTypeId));
        }

        [HttpGet]
        [Route("realms")]
        public async Task<RealmsViewModel> GetRealms()
        {
            return await mediator.Send(new GetRealmsCommand(OrganizationName));
        }

        [HttpGet]
        [Route("realms/{realmName}")]
        public async Task<RealmViewModel> GetRealm(string realmName)
        {
            return await mediator.Send(new GetRealmCommand(OrganizationName, realmName));
        }


        [HttpGet]
        [Route("realms/{realmName}/sections/{configurationSectionName}")]
        public async Task<ConfigurationSectionViewModel> GetConfigurationSection(
            string realmName,
            string configurationSectionName)
        {
            return await mediator.Send(
                new GetConfigurationSectionCommand(OrganizationName, realmName, configurationSectionName));
        }


        private static string ToJsonString(JsonDocument jdoc)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true});
            jdoc.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}