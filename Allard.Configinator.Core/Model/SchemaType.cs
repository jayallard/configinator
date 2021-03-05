using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Allard.Configinator.Core.Model
{
    [DebuggerDisplay("({SchemaTypeId.FullId)")]
    public record SchemaType(
        SchemaTypeId SchemaTypeId,
        IReadOnlyCollection<Property> Properties);

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId)")]
    public record Property(string Name, SchemaTypeId SchemaTypeId, bool IsSecret = false, bool IsOptional = false);
}