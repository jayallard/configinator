using System;

namespace Allard.Configinator.Core.Model
{
   public record SchemaType();

    public record SchemaTypeId(string Id, string Name) : ModelMemberId(Id, Name)
    {
        public static SchemaTypeId NewSchemaTypeId(string name)
        {
            return new (Guid.NewGuid().ToString(), name);
        }
    }
}