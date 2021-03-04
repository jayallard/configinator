using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Allard.Configinator.Core.Model
{
    [DebuggerDisplay("({SchemaTypeId.FullId)")]
    public record SchemaType(
        SchemaTypeId SchemaTypeId,
        IReadOnlyCollection<Property> Properties,
        IReadOnlyCollection<PropertyGroup> PropertyGroups);

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId)")]
    public record PropertyBase(string Name, SchemaTypeId SchemaTypeId, bool IsOptional)
    {
        public bool IsRequired => !IsOptional;
    }

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId)")]
    public record PropertyGroup(
        string Name,
        SchemaTypeId SchemaTypeId,
        bool IsOptional,
        IReadOnlyCollection<Property> Properties,
        IReadOnlyCollection<PropertyGroup> PropertyGroups) : PropertyBase(Name, SchemaTypeId, IsOptional);

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId)")]
    public record Property(string Name, SchemaTypeId SchemaTypeId, bool IsSecret = false, bool IsOptional = false) :
        PropertyBase(Name, SchemaTypeId, IsOptional);
}