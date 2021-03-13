using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public class ConfiginatorService : IConfiginatorService
    {
        private readonly IConfigStore configStore;
        private readonly IOrganizationRepository organizationRepository;

        public ConfiginatorService(IOrganizationRepository organizationRepository, IConfigStore configStore)
        {
            this.organizationRepository = organizationRepository;
            this.configStore = configStore;
        }

        public async Task<IConfiginator> GetConfiginatorByIdAsync(string organizationId)
        {
            var org = await organizationRepository.GetOrganizationByIdAsync(organizationId);
            return new Configinator(org, configStore);
        }

        public async Task<IConfiginator> GetConfiginatorByNameAsync(string organizationName)
        {
            var org = await organizationRepository.GetOrganizationByNameAsync(organizationName);
            return new Configinator(org, configStore);
        }

        public async Task<OrganizationAggregate> GetOrganizationByNameAsync(string organizationName)
        {
            return await organizationRepository.GetOrganizationByNameAsync(organizationName);
        }
    }
}