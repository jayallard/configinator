using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;

namespace Allard.Configinator.Blazor.Client.Services
{
    public class HttpOrganizationService : IOrganizationService
    {
        private readonly HttpClient client;
        private const string BaseUrl = "/api/v1/organizations/";

        public HttpOrganizationService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<OrganizationViewModel> GetOrganizationAsync(string organizationId)
        {
            var url = BaseUrl + HttpUtility.UrlEncode(organizationId);
            return await client.GetFromJsonAsync<OrganizationViewModel>(url);
        }

        public async Task<CreateOrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request)
        {
            var response = await client.PostAsJsonAsync(BaseUrl, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateOrganizationResponse>();
        }

        public async Task AddRealmToOrganizationAsync(string organizationId, RealmViewModel realm)
        {
            if (realm == null)
            {
                throw new ArgumentNullException(nameof(realm));
            }

            Console.WriteLine("service");
            Console.WriteLine(JsonSerializer.Serialize(realm));
            
            var url = BaseUrl + HttpUtility.UrlEncode(organizationId) + "/realms";
            await client.PostAsJsonAsync(url, realm);
        }
    }
}