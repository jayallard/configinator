using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Allard.Configinator.Schema
{
    // todo: collections aren't actually immutable... might be casted. fix.
    public record ConfigurationSchema(string Id, ReadOnlyCollection<PathNode> Paths);

    [DebuggerDisplay("{Path}")]
    public record PathNode(string Path, ReadOnlyCollection<Property> Properties);

    [DebuggerDisplay("{Name}")]
    public abstract record Property(string Name, SchemaParser.SchemaTypeId TypeId);

    [DebuggerDisplay("{Name}")]
    public record PropertyGroup
        (string Name, SchemaParser.SchemaTypeId TypeId, ReadOnlyCollection<Property> Properties) : Property(Name,
            TypeId);

    [DebuggerDisplay("{Name}")]
    public record PropertyPrimitive(string Name, SchemaParser.SchemaTypeId TypeId, bool IsSecret) : Property(Name, TypeId)
    {
        public PropertyPrimitive SetSecret(bool isSecret)
        {
            return this with {IsSecret = isSecret};
        }
    }
}