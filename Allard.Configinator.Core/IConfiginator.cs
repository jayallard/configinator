using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public interface IConfiginator
    {
        public OrganizationAggregate Organization { get; }
        public Task<SetValueResponse> SetValueAsync(SetValueRequest request);
        public Task<GetValueResponse> GetValueAsync(GetValueRequest request);
        public Task<GetDetailedValue> GetValueDetailAsync(GetValueRequest request);

    }
}