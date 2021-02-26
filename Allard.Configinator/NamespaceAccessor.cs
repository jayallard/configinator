using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Namespaces;

namespace Allard.Configinator
{
    public class NamespaceAccessor
    {
        private readonly INamespaceService service;

        public NamespaceAccessor(INamespaceService service)
        {
            this.service = service.EnsureValue(nameof(service));
        }

        public async Task<ConfigurationNamespace> ByName(string name)
        {
            return await service.GetNamespaceAsync(name).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ConfigurationNamespace>> All()
        {
            return await service.GetNamespacesAsync().ConfigureAwait(false);
        }
    }
}