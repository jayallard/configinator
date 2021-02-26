using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Allard.Configinator.Realms;
using Allard.Configinator.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;

namespace Allard.Configinator.Api.Controllers
{
    [ApiController]
    [Route("/api/v1")]
    public class ConfiginatorController
    {
        private readonly Configinator configinator;
        
        // todo: add accessor to configinator
        private readonly ISchemaService schemaService;
        

        public ConfiginatorController(Configinator configinator, ISchemaService schemaService)
        {
            this.configinator = configinator;
            this.schemaService = schemaService;
        }

        [HttpGet]
        [Route("/types/{typeId}")]
        public async Task<ObjectSchemaType> GetType(string typeId)
        {
            return await schemaService.GetSchemaTypeAsync(WebUtility.UrlDecode(typeId));
        }

        [HttpGet]
        [Route("/realms")]
        public async Task<IEnumerable<RealmStorageDto>> GetRealms()
        {
            return (await configinator.Realms.All())
                .Select(n => new RealmStorageDto
                {
                    Name = n.Name,
                    ConfigurationSections = n.ConfigurationSections.Select(cs => new RealmStorageDto.ConfigurationSectionStorageDto
                        {
                            Description = cs.Description,
                            Name = cs.Id.Name,
                            Path = cs.Path,
                            Type = cs.Type.SchemaTypeId.FullId
                        })
                        .ToList()
                });
        }
    }
}