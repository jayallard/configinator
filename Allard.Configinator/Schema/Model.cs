using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Allard.Configinator.Schema
{
    //public record SchemaType(string Name, ReadOnlyCollection<Property> Properties);
    
    [DebuggerDisplay("{Name}")]
    public abstract record Property(string Name, SchemaTypeId TypeId);

    [DebuggerDisplay("{Name}")]
    public record PropertyGroup
        (string Name, SchemaTypeId TypeId, ReadOnlyCollection<Property> Properties) : Property(Name,
            TypeId);

    [DebuggerDisplay("{Name}")]
    public record PropertyPrimitive(string Name, SchemaTypeId TypeId, bool IsSecret) : Property(Name, TypeId)
    {
        public PropertyPrimitive SetSecret(bool isSecret)
        {
            return this with {IsSecret = isSecret};
        }
    }
    
    /// <summary>
    /// Identity for a schema.
    /// Comprised of the schema name and the type name.
    /// </summary>
    [DebuggerDisplay("{FullId}")]
    public record SchemaTypeId
    {
        public bool IsPrimitive { get; }
        public string NameSpace { get; }
        public string TypeId { get; }
        public string FullId { get; }

        public SchemaTypeId(string fullId)
        {
            var parts = fullId.Split('/');
            NameSpace = parts[0];
            TypeId = parts[1];
            FullId = fullId;
            IsPrimitive = NameSpace == "primitive-types";
        }
    }
}