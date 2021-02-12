using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConfigurationManagement.Code.Schema
{
    // todo: collections aren't actually immutable... might be casted. fix.
    public record ConfigurationSchema
    {
        public string Id { get; init; }
        public ReadOnlyCollection<PathNode> Paths { get; init; }
    }

    [DebuggerDisplay("{Path}")]
    public record PathNode
    {
        public string Path { get; init; }
        public List<Property> Properties { get; init; }
    }

    [DebuggerDisplay("{Name}")]
    public abstract record Property
    {
        public string Name { get; init; }
    }

    [DebuggerDisplay("{Name}")]
    public record PropertyGroup : Property
    {
        public ReadOnlyCollection<Property> Properties { get; init; }
    }

    [DebuggerDisplay("{Name}")]
    public record PropertyPrimitive : Property
    {
        public bool IsSecret { get; init; }

        public string UnderlyingType { get; init; }

        public PropertyPrimitive SetSecret(bool isSecret)
        {
            return this with {IsSecret = isSecret};
        }
    }

    /*
    [DebuggerDisplay("{TypeId}")]
    public record SchemaType
    {
        public string TypeId { get; init; }
        public ReadOnlyCollection<Property> Properties { get; init; }
    }*/
}