using System.Collections.Generic;

namespace Allard.Configinator.Core.Model.Builders
{
    public class PropertyGroupBuilder
    {
        private readonly List<PropertyGroup> groups = new();
        private readonly List<Property> properties = new();

        public IReadOnlyCollection<PropertyGroup> Groups => groups.AsReadOnly();
        public IReadOnlyCollection<Property> Properties => properties.AsReadOnly();
        public string SchemaTypeId { get; protected set; }

        public bool Optional { get; protected set; }
        public string Name { get; protected set; }
        public PropertyGroupBuilder AddProperty(Property property)
        {
            properties.Add(property);
            return this;
        }

        public PropertyGroupBuilder AddPropertyGroup(PropertyGroup propertyGroup)
        {
            groups.Add(propertyGroup);
            return this;
        }

        public PropertyGroupBuilder AddProperty(string name, string schemaTypeId, bool isSecret = false,
            bool isOptional = false)
        {
            properties.Add(new Property(name, Model.SchemaTypeId.Parse(schemaTypeId), isSecret, isOptional));
            return this;
        }
        public static PropertyGroupBuilder Create(string name, string schemaTypeId)
        {
            return new() {Name = name, SchemaTypeId = schemaTypeId};
        }

        public static PropertyGroupBuilder Create(string name, SchemaTypeId schemaTypeId)
        {
            return new() {Name = name, SchemaTypeId = schemaTypeId.FullId};
        }

        public PropertyGroupBuilder IsOptional(bool isOptional)
        {
            Optional = isOptional;
            return this;
        }

        public PropertyGroup Build()
        {
            return new(Name, Model.SchemaTypeId.Parse(SchemaTypeId), Optional, Properties, Groups);
        }
    }
}