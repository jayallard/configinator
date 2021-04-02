using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Blazor.Client.Services
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationId>> GetOrganizationsAsync();
        Task<OrganizationViewModel> GetOrganizationAsync(string organizationId);
        Task<CreateOrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request);
        Task AddRealmToOrganizationAsync(string organizationId, RealmViewModel realm);
    }
}