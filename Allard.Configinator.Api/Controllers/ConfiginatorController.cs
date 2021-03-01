using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Api.Controllers
{
    [ApiController]
    [Route("/api/v1")]
    public class ConfiginatorController : Controller
    {
        private readonly LinkHelper links;
        private readonly IMediator mediator;

        public ConfiginatorController(IMediator mediator, LinkHelper links)
        {
            this.mediator = mediator;
            this.links = links;
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
            return await mediator.Send(new GetSchemaTypesCommand());
        }

        [HttpGet]
        [Route("schemaTypes/{typeId}")]
        public async Task<SchemaTypeViewModel> GetSchemaType(string typeId)
        {
            return await mediator.Send(new GetSchemaTypeCommand(typeId));
        }

        [HttpGet]
        [Route("realms")]
        public async Task<RealmsViewModel> GetRealms()
        {
            return await mediator.Send(new GetRealmsCommand());
        }

        [HttpGet]
        [Route("realms/{name}")]
        public async Task<RealmViewModel> GetRealm(string name)
        {
            return await mediator.Send(new GetRealmCommand(name));
        }

        [HttpGet]
        [Route("realms/{realmName}/sections/{configurationSectionName}")]
        public async Task<ConfigurationSectionViewModel> GetConfigurationSection(string realmName,
            string configurationSectionName)
        {
            return await mediator.Send(new GetConfigurationSectionCommand(realmName, configurationSectionName));
        }

        [HttpGet]
        [Route("realms/{realmName}/sections/{configurationSectionName}/value/{habitat}")]
        public async Task<ConfigurationValue> GetConfigurationValue(
            string realmName,
            string configurationSectionName,
            string habitat)
        {
            var value = await mediator.Send(new GetConfigurationValueCommand(habitat, realmName,
                configurationSectionName));
            ;
            return value;
        }

        [HttpGet]
        [Route("realms/{realmName}/sections/{configurationSectionName}/value/{habitat}/resolved")]
        public async Task<JsonDocument> GetConfigurationValueResolved(
            string realmName,
            string configurationSectionName,
            string habitat)
        {
            var value = await mediator.Send(new GetConfigurationValueCommand(habitat, realmName,
                configurationSectionName));
            ;
            return JsonDocument.Parse(value.ResolvedValue);
        }

        private static string ToJsonString(JsonDocument jdoc)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true});
            jdoc.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        [HttpPut]
        [Route("realms/{realmName}/sections/{configurationSectionName}/value/{habitat}")]
        public async Task<Blah> SetConfigurationValue(
            string realmName,
            string configurationSectionName,
            string habitat,
            [FromBody] SetValueRequestModel value)
        {
            return await mediator.Send(
                new SetConfigurationValueCommand
                {
                    PreviousEtag = value.PreviousETag,
                    Habitat = habitat,
                    Realm = realmName,
                    ConfigurationSection = configurationSectionName,
                    Value = value.Value
                });
        }
    }
}