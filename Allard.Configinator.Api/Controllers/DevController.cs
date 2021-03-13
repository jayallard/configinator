using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Infrastructure.MongoDb;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Api.Controllers
{
    [ApiController]
    [Route("/api/v1")]
    public class DevController
    {
        private readonly OrganizationRepositoryMongo organizationRepository;

        public DevController(IOrganizationRepository organizationRepository)
        {
            this.organizationRepository = (OrganizationRepositoryMongo) organizationRepository;
        }

        [HttpPost]
        [Route("dev/reset")]
        public async Task Reset()
        {
            await organizationRepository.DevelopmentSetup();
        }
    }
}