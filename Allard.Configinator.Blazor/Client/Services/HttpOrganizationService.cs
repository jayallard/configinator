using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Allard.Configinator.Blazor.Shared.ViewModels;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;

namespace Allard.Configinator.Blazor.Client.Services
{
    public class HttpOrganizationService : IOrganizationService
    {
        private readonly HttpClient client;

        public HttpOrganizationService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<OrganizationViewModel> GetOrganizationAsync(string organizationId)
        {
            var url = "/api/v1/organizations/" + organizationId;
            return await client.GetFromJsonAsync<OrganizationViewModel>(url);
        }
    }
}