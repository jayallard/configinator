using System.Threading.Tasks;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public interface IConfiginatorService
    {
        public Task<IConfiginator> GetConfiginatorByIdAsync(string organizationId);
        public Task<IConfiginator> GetConfiginatorByNameAsync(string organizationName);

        public Task<OrganizationAggregate> GetOrganizationByNameAsync(string organizationName);
    }
}