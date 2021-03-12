using System.Collections.Generic;

namespace Allard.Configinator.Core.Model.Builders
{
    public class SchemaTypeBuilder
    {
        private readonly List<SchemaTypeProperty> properties = new();

        public IReadOnlyCollection<SchemaTypeProperty> Properties => properties.AsReadOnly();

        public string TypeId { get; protected set; }

        public static SchemaTypeBuilder Create()
        {
            return new();
        }

        public SchemaTypeBuilder AddProperty(SchemaTypeProperty schemaTypeProperty)
        {
            properties.Add(schemaTypeProperty);
            return this;
        }

        public SchemaTypeBuilder AddProperty(string name, string schemaTypeId, bool isSecret = false,
            bool isOptional = false)
        {
            properties.Add(new SchemaTypeProperty(name, SchemaTypeId.Parse(schemaTypeId), isSecret, isOptional));
            return this;
        }

        public SchemaTypeBuilder AddProperty(string name, SchemaTypeId schemaTypeId, bool isSecret = false,
            bool isOptional = false)
        {
            properties.Add(new SchemaTypeProperty(name, schemaTypeId, isSecret, isOptional));
            return this;
        }

        public SchemaTypeBuilder AddStringProperty(string name, bool isSecret = false, bool isOptional = false)
        {
            return AddProperty(name, SchemaTypeId.String, isSecret, isOptional);
        }

        public SchemaType Build()
        {
            return new(SchemaTypeId.Parse(TypeId), Properties);
        }

        public static SchemaTypeBuilder Create(string schemaTypeId)
        {
            return new() {TypeId = schemaTypeId};
        }
    }
}