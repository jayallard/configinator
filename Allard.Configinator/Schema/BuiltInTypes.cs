using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Schema
{
    public static class BuiltInTypes
    {
        private static readonly ObjectSchemaType stringType =
            new (new SchemaTypeId("string"), new List<Property>().AsReadOnly());

        public static int Count => 1;
        public static ObjectSchemaType Get(SchemaTypeId typeId)
        {
            return typeId == stringType.SchemaTypeId
                ? stringType
                : null;
        }
    }
}