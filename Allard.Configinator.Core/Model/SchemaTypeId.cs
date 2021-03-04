using System;

namespace Allard.Configinator.Core.Model
{
    public record SchemaTypeId
    {
        public static SchemaTypeId CreatePrimitive(string type)
        {
            return new SchemaTypeId("primitive", type);
        }

        public static SchemaTypeId Parse(string fullId)
        {
            fullId.EnsureValue(nameof(fullId));
            var parts = fullId.Split("/");
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid type id: " + fullId);
            }

            return Create(parts[0], parts[1]);
        }
        public static SchemaTypeId Create(string Namespace, string type)
        {
            return new SchemaTypeId(Namespace, type);
        }

        public string Namespace { get; }

        public string Id { get; }

        public string FullId { get; }

        public SchemaTypeId(string namesSpace, string id)
        {
            Namespace = namesSpace.ToNormalizedMemberName(namesSpace);
            Id = id.ToNormalizedMemberName(namesSpace);
            FullId = namesSpace + "/" + id;
        }
    }
}