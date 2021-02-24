using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     The properties of a type.
    /// </summary>
    [DebuggerDisplay("{SchemaTypeId}")]
    public record ObjectSchemaType(SchemaTypeId SchemaTypeId, ReadOnlyCollection<Property> Properties);
    
    [DebuggerDisplay("{Name}")]
    public abstract record Property(string Name, SchemaTypeId TypeId, bool IsOptional)
    {
        public bool IsRequired => !IsOptional;
    }

    [DebuggerDisplay("{Name}")]
    public record PropertyGroup(string Name, SchemaTypeId TypeId, bool IsOptional,
        ReadOnlyCollection<Property> Properties) : Property(Name, TypeId, IsOptional);

    [DebuggerDisplay("{Name}")]
    public record PropertyPrimitive(string Name, SchemaTypeId TypeId, bool IsSecret, bool IsOptional) : Property(Name, TypeId, IsOptional)
    {
    }

    /// <summary>
    ///     Identity for a schema.
    ///     Comprised of the schema name and the type name.
    /// </summary>
    [DebuggerDisplay("{FullId}")]
    public record SchemaTypeId
    {
        public SchemaTypeId(string fullId)
        {
            fullId = string.IsNullOrWhiteSpace(fullId)
                ? throw new ArgumentNullException(nameof(fullId))
                : fullId;

            if (!fullId.Contains("/"))
            {
                NameSpace = string.Empty;
                TypeId = fullId;
                FullId = fullId;
                IsPrimitive = true;
                return;
            }

            var parts = fullId.Split('/');
            NameSpace = parts[0];
            TypeId = parts[1];
            FullId = fullId;
            IsPrimitive = false;
        }

        public bool IsPrimitive { get; }
        public string NameSpace { get; }
        public string TypeId { get; }
        public string FullId { get; }
    }
}