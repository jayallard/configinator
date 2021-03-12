using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public interface IConfiginator
    {
        public OrganizationAggregate Organization { get; }
        public Task<SetConfigurationResponse> SetValueResolvedAsync(SetConfigurationRequest request);

        public Task<SetConfigurationResponse> SetValueRawAsync(SetConfigurationRequest request);
        public Task<GetConfigurationResponse> GetValueResolvedAsync(GetConfigurationRequest request);
        public Task<GetConfigurationResponse> GetValueRawAsync(GetConfigurationRequest request);
    }
}