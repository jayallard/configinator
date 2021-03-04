using System.Collections.Generic;

namespace Allard.Configinator.Core.Model.Builders
{
    public class SchemaTypeBuilder
    {
        private readonly List<PropertyGroup> groups = new();
        private readonly List<Property> properties = new();

        public IReadOnlyCollection<PropertyGroup> Groups => groups.AsReadOnly();
        public IReadOnlyCollection<Property> Properties => properties.AsReadOnly();

        public SchemaTypeBuilder AddProperty(Property property)
        {
            properties.Add(property);
            return this;
        }

        public SchemaTypeBuilder AddPropertyGroup(PropertyGroup propertyGroup)
        {
            groups.Add(propertyGroup);
            return this;
        }

        public SchemaTypeBuilder AddProperty(string name, string schemaTypeId, bool isSecret = false,
            bool isOptional = false)
        {
            properties.Add(new Property(name, Model.SchemaTypeId.Parse(schemaTypeId), isSecret, isOptional));
            return this;
        }

        public SchemaTypeBuilder AddProperty(string name, SchemaTypeId schemaTypeId, bool isSecret = false,
            bool isOptional = false)
        {
            properties.Add(new Property(name, schemaTypeId, isSecret, isOptional));
            return this;
        }

        public string TypeId { get; protected set; }

        public SchemaType Build()
        {
            return new(SchemaTypeId.Parse(TypeId), Properties, Groups);
        }

        public static SchemaTypeBuilder Create(string schemaTypeId)
        {
            return new() {TypeId = schemaTypeId};
        }
    }
}