using System.Collections.Generic;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Infrastructure
{
    public interface IOrganizationQueries
    {
        // TODO: task
        public IEnumerable<OrganizationId> GetOrganizationIds();
    }
}