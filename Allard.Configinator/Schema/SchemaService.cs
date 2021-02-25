using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
            await Load();
            if (types.TryGetValue(typeId, out var type))
            {
                return type;
            }

            throw new SchemaTypeDoesntExistException(typeId);
        }

        private async Task Load()
        {
            var typesDto = await repository.GetSchemaTypes();
            var resolved = await SchemaResolver.ConvertAsync(typesDto);
            types = resolved.ToDictionary(r => r.SchemaTypeId.FullId);
        }
    }

    public class SchemaTypeDoesntExistException : Exception
    {
        public string TypeName { get; }
        public SchemaTypeDoesntExistException(string typeName)
        {
            TypeName = typeName;
        }
    }
}