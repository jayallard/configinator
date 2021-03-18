using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;

namespace Allard.Configinator.Blazor.Client.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationViewModel> GetOrganizationAsync(string organizationId);
    }
}