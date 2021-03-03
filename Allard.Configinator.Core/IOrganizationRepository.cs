using System.Threading.Tasks;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public interface IOrganizationRepository
    {
        Task<Organization> GetOrganizationAsync(string id);
        Task SaveAsync(Organization organization);
    }
}