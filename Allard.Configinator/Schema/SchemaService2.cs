using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allard.Configinator.Schema
{
    public class SchemaService2 : ISchemaService
    {
        private readonly ISchemaRepository repository;
        private Dictionary<string, ObjectSchemaType> types;
        public SchemaService2(ISchemaRepository repository)
        {
           this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        private async Task Load()
        {
            await Task.Run(() => { });
            //var typeDtos = await repository.GetSchemaTypes();
        }

        public async Task<ObjectSchemaType> GetSchemaTypeAsync(string typeId)
        {
            if (types == null)
            {
                await Load();
            }
            throw new System.NotImplementedException();
        }
    }
}