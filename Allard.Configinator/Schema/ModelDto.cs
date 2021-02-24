using System.Collections.Generic;
using System.Diagnostics;

namespace Allard.Configinator.Schema
{
    /// <summary>
    /// Classes used for the transfer of information about schemas.
    /// IE: Data may be stored in this format in a database or in the file system.
    /// </summary>
    public static class ModelDto
    {
        [DebuggerDisplay("{TypeName}")]
        public class TypeDto
        {
            public string Namespace { get; set; }
            public string TypeName { get; set; }
            public string BaseTypeName { get; set; }
            public IList<PropertyDto> Properties { get; set; } 
            public HashSet<string> Secrets { get; set; }
            public HashSet<string> Optional { get; set; }
        }

        [DebuggerDisplay("{PropertyName}: {TypeName}")]
        public class PropertyDto
        {
            public string TypeName { get; set; }
            public string PropertyName { get; set; }
            public bool IsOptional { get; set; }
            public bool IsSecret { get; set; }
        }
    }
}