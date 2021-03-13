using System.Threading.Tasks;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Infrastructure
{
    public interface IOrganizationRepository
    {
        Task<OrganizationAggregate> GetOrganizationByIdAsync(string id);
        Task<OrganizationAggregate> GetOrganizationByNameAsync(string name);
        Task SaveAsync(OrganizationAggregate organization);
    }
}