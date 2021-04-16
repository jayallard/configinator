using System.Collections.Generic;
using System.Diagnostics;

namespace Allard.Configinator.Core.Model
{
    public record HabitatId(string Id) : ModelMemberId(Id);

    public record RealmId(string Id) : ModelMemberId(Id);

    public record SectionId(string Id) : ModelMemberId(Id);

    public record OrganizationId(string Id) : ModelMemberId(Id);

    public abstract record ModelMemberId
    {
        protected ModelMemberId(string id)
        {
            Id = id.ToNormalizedMemberName(nameof(id));
        }

        public string Id { get; }
    }

    [DebuggerDisplay("{SchemaTypeId.FullId}")]
    public record SchemaType(
        SchemaTypeId SchemaTypeId,
        IReadOnlyCollection<SchemaTypeProperty> Properties);

    [DebuggerDisplay("{SchemaTypeId.FullId}")]
    public record SchemaTypeExploded(
        SchemaTypeId SchemaTypeId,
        IReadOnlyCollection<SchemaTypePropertyExploded> Properties);

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId})")]
    public record SchemaTypeProperty(string Name, SchemaTypeId SchemaTypeId, bool IsSecret = false,
        bool IsOptional = false)
    {
        public bool IsRequired => !IsOptional;
    }

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId})")]
    public record SchemaTypePropertyExploded(string Name, SchemaTypeId SchemaTypeId,
        IReadOnlyCollection<SchemaTypePropertyExploded> Properties, bool IsSecret, bool IsOptional)
    {
        public bool IsRequired => !IsOptional;
    }
    

    public record RealmVariable(string Name, string SectionId, string ConfigPath,
        List<RealmVariableAssignment> Assignments);
    public record RealmVariableAssignment(string ConfigurationSectionId, string ConfigPath);
}