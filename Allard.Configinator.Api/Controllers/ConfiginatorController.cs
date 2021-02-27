using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Schema;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Api.Controllers
{
    [ApiController]
    [Route("/api/v1")]
    public class ConfiginatorController
    {
        private readonly IMediator mediator;

        public ConfiginatorController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("types/{typeId}")]
        public async Task<SchemaTypeViewModel> GetType(string typeId)
        {
            return await mediator.Send(new GetSchemaTypeCommand(typeId));
        }

        [HttpGet]
        [Route("realms")]
        public async Task<IEnumerable<RealmViewModel>> GetRealms()
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
    }
}