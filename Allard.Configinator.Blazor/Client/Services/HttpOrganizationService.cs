using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        public async Task<CreateOrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request)
        {
            const string url = "/api/v1/organizations";
            var response = await client.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateOrganizationResponse>();
        }
    }
}