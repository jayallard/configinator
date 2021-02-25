using System.Collections.Generic;
using System.Diagnostics;

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     Classes used for the transfer of information about schemas.
    ///     IE: Data may be stored in this format in a database or in the file system.
    /// </summary>
    public static class ModelDto
    {
        public abstract class PropertyContainer
        {
            public HashSet<string> Secrets { get; set; }
            public HashSet<string> Optional { get; set; }
            public IList<PropertyDto> Properties { get; set; }
            public string TypeName { get; set; }
        }

        [DebuggerDisplay("{TypeName}")]
        public class TypeDto : PropertyContainer
        {
            public string Namespace { get; set; }
            public string BaseTypeName { get; set; }
        }

        [DebuggerDisplay("{PropertyName}: {TypeName}")]
        public class PropertyDto : PropertyContainer
        {
            public string PropertyName { get; set; }
            public bool IsOptional { get; set; }

            /// <summary>
            ///     Gets or sets a value indicating whether this property is a
            ///     secret. This only applies to simple properties, not to groups.
            /// </summary>
            public bool IsSecret { get; set; }
        }
    }
}