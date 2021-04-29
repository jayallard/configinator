using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Infrastructure
{
    public interface IOrganizationQueries
    {
        // TODO: task
        public Task<IEnumerable<OrganizationId>> GetOrganizationIds();
    }
}