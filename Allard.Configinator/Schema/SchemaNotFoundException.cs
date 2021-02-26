using System;

namespace Allard.Configinator.Schema
{
    public class SchemaNotFoundException : Exception
    {
        public SchemaNotFoundException(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }
    }
}