using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema.Validator;

namespace Allard.Configinator.Schema
{
    public class SchemaService : ISchemaService
    {
        private readonly ISchemaRepository repository;
        private readonly ISchemaValidator validator;
        private Dictionary<string, ObjectSchemaType> types;

        public SchemaService(ISchemaRepository repository, ISchemaValidator validator)
        {
            this.repository = repository.EnsureValue(nameof(repository));
            this.validator = validator.EnsureValue(nameof(validator));
        }

        public async Task<ObjectSchemaType> GetSchemaTypeAsync(string typeId)
        {
            await Load().ConfigureAwait(false);
            if (types.TryGetValue(typeId, out var type)) return type;

            throw new SchemaNotFoundException(typeId);
        }

        public async Task<IEnumerable<ObjectSchemaType>> GetSchemaTypesAsync()
        {
            await Load().ConfigureAwait(false);
            return types.Values;
        }

        private async Task Load()
        {
            var typesDto = await repository.GetSchemaTypes().ConfigureAwait(false);
            var resolved = await SchemaResolver.ConvertAsync(typesDto).ConfigureAwait(false);
            types = resolved.ToDictionary(r => r.SchemaTypeId.FullId);
        }
    }
}