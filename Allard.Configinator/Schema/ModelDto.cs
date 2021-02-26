using System.Collections.Generic;
using System.Diagnostics;

namespace Allard.Configinator.Schema
{
    [DebuggerDisplay("{PropertyName}: {TypeName}")]
    public class PropertyDto : PropertyContainer
    {
        public string PropertyName { get; init; }
        public bool IsOptional { get; init; }

        /// <summary>
        ///     Gets or sets a value indicating whether this property is a
        ///     secret. This only applies to simple properties, not to groups.
        /// </summary>
        public bool IsSecret { get; init; }
    }

    [DebuggerDisplay("{TypeName}")]
    public class TypeDto : PropertyContainer
    {
        public string Namespace { get; init; }
        public string BaseTypeName { get; init; }
    }

    public abstract class PropertyContainer
    {
        public HashSet<string> Secrets { get; init; }
        public HashSet<string> Optional { get; init; }
        public IList<PropertyDto> Properties { get; init; }
        public string TypeName { get; init; }
    }
}