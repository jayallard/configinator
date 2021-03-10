using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public interface IConfiginator
    {
        public OrganizationAggregate Organization { get; }
        public Task<SetConfigurationResponse> SetValueAsync(SetConfigurationRequest request);
        public Task<GetConfigurationResponse> GetValueAsync(GetConfigurationRequest request);
    }
}