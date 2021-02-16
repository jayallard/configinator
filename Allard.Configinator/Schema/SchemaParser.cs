using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public class SchemaParser
    {
        /// <summary>
        /// Stores the YAML per schema id.
        /// </summary>
        private readonly Dictionary<string, YamlMappingNode> sourceYaml = new();

        /// <summary>
        /// Stores the schemas per schema id.
        /// </summary>
        private readonly Dictionary<string, ConfigurationSchema> schemas = new();

        /// <summary>
        /// Stores the schema types per type id.
        /// </summary>
        private readonly Dictionary<SchemaTypeId, ObjectSchemaType> types = new();

        /// <summary>
        /// Retrieves the Yaml.
        /// </summary>
        private readonly ISchemaRepository repository;

        /// <summary>
        /// Converts YamlProperties to property objects.
        /// </summary>
        private readonly PropertyParser propertyParser;

        /// <summary>
        /// Initializes an instance of the SchemaParser class.
        /// </summary>
        /// <param name="repository"></param>
        public SchemaParser(ISchemaRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            propertyParser = new PropertyParser(this);
        }

        /// <summary>
        /// Retrieve a schema.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ConfigurationSchema> GetSchema(string id)
        {
            if (schemas.ContainsKey(id))
            {
                return schemas[id];
            }

            var schema = await BuildSchema(id);
            schemas[id] = schema;
            return schema;
        }

        /// <summary>
        /// Convert YAML to a ConfigurationSchema object.
        /// </summary>
        /// <param name="schemaId">The id of the schema to convert.</param>
        /// <returns></returns>
        private async Task<ConfigurationSchema> BuildSchema(string schemaId)
        {
            var source = await GetYaml(schemaId);
            var pathNodes = (YamlMappingNode) source["paths"];
            var paths = new List<PathNode>();
            foreach (var p in pathNodes)
            {
                var properties = await propertyParser
                    .GetProperties(schemaId, (YamlMappingNode) p.Value);
                paths.Add(new PathNode((string) p.Key, properties.ToList().AsReadOnly()));
            }

            return new ConfigurationSchema(schemaId, paths.AsReadOnly());
        }

        /// <summary>
        /// Converts a type id to a standard format.
        /// If the id is "./type", it is converted to "relativeSchemaId/type".
        /// If the id starts with "/x", it is converted to "primitive-types/x",
        /// where x can be any primitive type.
        /// </summary>
        /// <param name="relativeSchemaId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        private static SchemaTypeId NormalizeTypeId(string relativeSchemaId, string typeId)
        {
            if (!typeId.Contains("/"))
            {
                // if a schema isn't specified (no / in the id),
                // then set it to primitive-types
                return new SchemaTypeId("primitive-types/" + typeId);
            }

            if (typeId.StartsWith("./"))
            {
                // if references this schema, then change the ./ to this schema id.
                // IE:  "./typeId" becomes "currentSchemaId/typeId"
                return new SchemaTypeId(relativeSchemaId + "/" + typeId.Substring(2));
            }

            return new SchemaTypeId(typeId);
        }

        /// <summary>
        /// Returns the schema type of the given id.
        /// </summary>
        /// <param name="typeId">The id of the schema type to return.</param>
        /// <returns></returns>
        private async Task<ObjectSchemaType> GetSchemaType(SchemaTypeId typeId)
        {
            if (types.ContainsKey(typeId))
            {
                return types[typeId];
            }

            var type = await BuildType(typeId);
            types[typeId] = type;
            return type;
        }

        /// <summary>
        /// Build a schema type from yaml.
        /// </summary>
        /// <param name="schemaTypeId"></param>
        /// <returns></returns>
        private async Task<ObjectSchemaType> BuildType(SchemaTypeId schemaTypeId)
        {
            var sourceSchema = await GetYaml(schemaTypeId.SchemaId);
            var typesYaml = (YamlMappingNode) sourceSchema["types"];
            var typeYaml = typesYaml.Single(t => (string) t.Key == schemaTypeId.TypeId);
            var props = new List<Property>();

            // from the base, if there is one.
            var baseTypeName = typeYaml.Value.ChildAsString("$base");
            if (baseTypeName != null)
            {
                var baseTypeId = NormalizeTypeId(schemaTypeId.SchemaId, baseTypeName);
                var baseType = await GetSchemaType(baseTypeId);
                props.AddRange(baseType.Properties);
            }

            // from the local type
            props.AddRange(await propertyParser.GetProperties(schemaTypeId.SchemaId, (YamlMappingNode) typeYaml.Value));
            return new ObjectSchemaType(schemaTypeId, props.AsReadOnly());
        }

        /// <summary>
        /// Get the Yaml for the schema.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<YamlMappingNode> GetYaml(string schemaId)
        {
            if (sourceYaml.ContainsKey(schemaId))
            {
                return sourceYaml[schemaId];
            }

            var source = await repository.GetSchema(schemaId);

            // make sure the id from the source matches the requested id.
            var sourceId = (string) source["id"];
            if (schemaId != sourceId)
            {
                throw new InvalidOperationException($"Schema id mismatch. Schema Id={schemaId}, Id in File={sourceId}");
            }

            sourceYaml[schemaId] = source;
            return sourceYaml[schemaId];
        }

        [DebuggerDisplay("{SchemaTypeId}")]
        private record ObjectSchemaType(SchemaTypeId SchemaTypeId, ReadOnlyCollection<Property> Properties);

        [DebuggerDisplay("{FullId}")]
        public record SchemaTypeId
        {
            public bool IsPrimitive { get; }
            public string SchemaId { get; }
            public string TypeId { get; }
            public string FullId { get; }

            public SchemaTypeId(string fullId)
            {
                var parts = fullId.Split('/');
                SchemaId = parts[0];
                TypeId = parts[1];
                FullId = fullId;
                IsPrimitive = SchemaId == "primitive-types";
            }
        }

        private class PropertyParser
        {
            private readonly SchemaParser schemaParser;

            public PropertyParser(SchemaParser schemaParser)
            {
                this.schemaParser = schemaParser;
            }

            public async Task<IEnumerable<Property>> GetProperties(string schemaId, YamlMappingNode yaml)
            {
                var properties = new List<Property>();
                var propertiesYaml = (YamlMappingNode) yaml["properties"];
                var secrets = yaml.ChildAsHashSet("secrets");
                foreach (var p in propertiesYaml)
                {
                    //  Type may be specified 2 ways.
                    //       properties:
                    //          "value": "string"
                    //          "object":
                    //             "type": "string"
                    var isObject = p.Value is YamlMappingNode;
                    var typeIdName = isObject
                        ? (string) ((YamlMappingNode) p.Value)["type"]
                        : (string) p.Value;

                    var propertyName = (string) p.Key;
                    var typeId = NormalizeTypeId(schemaId, typeIdName);
                    if (typeId.IsPrimitive)
                    {
                        var isSecret = secrets.Contains(propertyName) || p.Value.ChildAsBoolean("is-secret");
                        properties.Add(new PropertyPrimitive(propertyName, typeId, isSecret));
                        continue;
                    }

                    // get the properties for the type.
                    // append additional propertiees added at
                    // the schema leel.
                    var propertiesForType = new List<Property>();
                    var type = await schemaParser.GetSchemaType(typeId);
                    propertiesForType.AddRange(type.Properties);
                    properties.Add(new PropertyGroup(propertyName, typeId, propertiesForType.AsReadOnly()));
                }

                // make sure that all properties specified in the "secrets" collection
                // are valid.
                // IE:    secrets: ["a", "b", "c"]
                // confirm that a,b,c are all valid property names.
                var propertyNames = properties.Select(p => p.Name).ToHashSet();
                var badNames = secrets.Where(s => !propertyNames.Contains(s)).ToList();
                if (badNames.Count == 0)
                {
                    return properties;
                }

                var badNamesCombined = string.Join(',', badNames);
                throw new InvalidOperationException("Invalid secret names: " + badNamesCombined);
            }
        }
    }
}