using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allard.Configinator.Schema
{
    public class SchemaService : ISchemaService
    {
        private readonly ISchemaRepository repository;
        private Dictionary<string, ObjectSchemaType> types;

        public SchemaService(ISchemaRepository repository)
        {
            this.repository = repository.EnsureValue(nameof(repository));
        }

        public async Task<ObjectSchemaType> GetSchemaTypeAsync(string typeId)
        {
            await Load().ConfigureAwait(false);
            if (types.TryGetValue(typeId, out var type)) return type;

            throw new SchemaNotFoundException(typeId);
        }

        private async Task Load()
        {
            var typesDto = await repository.GetSchemaTypes().ConfigureAwait(false);
            var resolved = await SchemaResolver.ConvertAsync(typesDto).ConfigureAwait(false);
            types = resolved.ToDictionary(r => r.SchemaTypeId.FullId);
        }
    }
}