using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Blazor.Shared.ViewModels;

namespace Allard.Configinator.Blazor.Client.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationViewModel> GetOrganizationAsync(string organizationId);
    }
}