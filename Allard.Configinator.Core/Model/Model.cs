using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
    public class SchemaTypeExploded
    {
        private readonly Dictionary<string, SchemaTypePropertyExploded> propertiesMap;
        public SchemaTypeId SchemaTypeId { get; }
        public IReadOnlyCollection<SchemaTypePropertyExploded> Properties => propertiesMap.Values;

        public SchemaTypeExploded(SchemaTypeId schemaTypeId,
            IEnumerable<SchemaTypePropertyExploded> properties)
        {
            SchemaTypeId = schemaTypeId.EnsureValue(nameof(schemaTypeId));
            propertiesMap = properties
                .EnsureValue(nameof(properties))
                .ToDictionary(p => p.Name);
        }

        public SchemaTypePropertyExploded GetProperty(string name)
        {
            return propertiesMap[name];
        }
    }

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId})")]
    public record SchemaTypeProperty(string Name, SchemaTypeId SchemaTypeId, bool IsSecret = false,
        bool IsOptional = false)
    {
        public bool IsRequired => !IsOptional;
    }

    [DebuggerDisplay("{Name} ({SchemaTypeId.FullId})")]
    public class SchemaTypePropertyExploded
    {
        private readonly Dictionary<string, SchemaTypePropertyExploded> propertiesMap;

        public SchemaTypePropertyExploded(
            string name,
            SchemaTypeId schemaTypeId,
            IEnumerable<SchemaTypePropertyExploded> properties,
            bool isSecret,
            bool isOptional)
        {
            Name = name;
            SchemaTypeId = schemaTypeId;
            propertiesMap = properties.ToDictionary(p => p.Name);
            IsSecret = isSecret;
            IsOptional = isOptional;
        }

        public bool IsRequired => !IsOptional;
        public string Name { get; init; }
        public SchemaTypeId SchemaTypeId { get; init; }
        public IReadOnlyCollection<SchemaTypePropertyExploded> Properties => propertiesMap.Values;
        public bool IsSecret { get; }
        public bool IsOptional { get; }

        public SchemaTypePropertyExploded GetProperty(string propertyName)
        {
            return propertiesMap[propertyName];
        }

        public bool PropertyExists(string propertyName)
        {
            return propertiesMap.ContainsKey(propertyName);
        }
    }


    public record RealmVariable(string Name, SchemaTypeId SchemaTypeId, List<RealmVariableAssignment> Assignments);

    public record RealmVariableAssignment(string SectionId, string ConfigPath);
}