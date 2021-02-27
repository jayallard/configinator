using System;

namespace Allard.Configinator.Schema
{
    public class SchemaNotFoundException : Exception
    {
        public SchemaNotFoundException(string typeId):base("Schema not found for type: " + typeId)
        {
            TypeId = typeId;
        }

        public string TypeId { get; }
    }
}