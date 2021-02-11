using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ConfigurationManagement.Code.Schema
{
    // todo: collections aren't actually immutable... might be casted. fix.
    public record ConfigurationSchema
    {
        public string Id { get; init; }
        public ReadOnlyCollection<PathNode> Paths { get; init; }
    }

    public class PathNode
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
    public record PropertyValue : Property
    {
        public bool IsSecret { get; init; }

        public PropertyValue SetSecret(bool isSecret)
        {
            return this with {IsSecret = isSecret};
        }
    }

    [DebuggerDisplay("{TypeId}")]
    public record SchemaType
    {
        public string TypeId { get; init; }
        public ReadOnlyCollection<Property> Properties { get; init; }
    }
}