using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Infrastructure
{
    public interface IOrganizationRepository
    {
        Task<OrganizationAggregate> GetOrganizationByIdAsync(string id);
        Task CreateAsync(OrganizationAggregate organization);
        Task UpdateAsync(OrganizationAggregate organization);
    }
}