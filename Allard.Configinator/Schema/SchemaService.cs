using System;
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
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ObjectSchemaType> GetSchemaTypeAsync(string typeId)
        {
            if (types == null) await Load();

            return types[typeId];
        }

        private async Task Load()
        {
            var typesDto = await repository.GetSchemaTypes();
            var resolved = await SchemaResolver.ConvertAsync(typesDto);
            types = resolved.ToDictionary(r => r.SchemaTypeId.FullId);
        }
    }
}