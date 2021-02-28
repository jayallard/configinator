using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers.ViewModels;
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
        private readonly LinkHelper links;

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
    }
}