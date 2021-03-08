using System.Threading.Tasks;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Infrastructure
{
    public interface IOrganizationRepository
    {
        Task<OrganizationAggregate> GetOrganizationAsync(string id);
        Task SaveAsync(OrganizationAggregate organization);
    }
}