using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    /// <summary>
    /// Converts Yaml to a 
    /// </summary>
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
        private readonly Dictionary<SchemaTypeId, ObjectSchemaType> schemaTypes = new();

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
        /// The node names that are valid within a path node.
        /// "paths":
        ///     "/x/y/z":
        ///         these nodes
        /// </summary>
        private readonly HashSet<string> allowedPathNodeNames = new(new[] {"$type", "properties", "secrets"});
        
        /// <summary>
        /// The node names that are valid within a type node
        /// "types":
        ///     "my-type":
        ///         these nodes
        /// </summary>
        private readonly HashSet<string> allowedTypeNodeName = new(new[] {"$base", "properties", "secrets"});

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
                EnsureValuesAreValid("PATH node has invalid children.", p.Value.ChildNames(), allowedPathNodeNames);
                var properties = await propertyParser.GetProperties(schemaId, p.Value);
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
            if (schemaTypes.ContainsKey(typeId))
            {
                return schemaTypes[typeId];
            }

            var type = await BuildType(typeId);
            schemaTypes[typeId] = type;
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
            EnsureValuesAreValid("TYPE node has invalid children.", typeYaml.Value.ChildNames(), allowedTypeNodeName);
            var props = new List<Property>();

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

            var source = await repository.GetSchemaYaml(schemaId);

            // make sure the id from the source matches the requested id.
            var sourceId = (string) source["id"];
            if (schemaId != sourceId)
            {
                throw new InvalidOperationException($"Schema id mismatch. Schema Id={schemaId}, Id in File={sourceId}");
            }

            sourceYaml[schemaId] = source;
            return sourceYaml[schemaId];
        }

        /// <summary>
        /// The properties of a type.
        /// </summary>
        [DebuggerDisplay("{SchemaTypeId}")]
        private record ObjectSchemaType(SchemaTypeId SchemaTypeId, ReadOnlyCollection<Property> Properties);

        /// <summary>
        /// Identity for a schema.
        /// Comprised of the schema name and the type name.
        /// </summary>
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

        /// <summary>
        /// Parses properties for paths and for types.
        /// It collects properties from base type
        /// and reference types, etc. The whole gambit.
        /// </summary>
        private class PropertyParser
        {
            /// <summary>
            /// Used to retrieve other types.
            /// </summary>
            private readonly SchemaParser schemaParser;

            /// <summary>
            /// Initializes a new instance of the PropertyParser class.
            /// </summary>
            /// <param name="schemaParser"></param>
            public PropertyParser(SchemaParser schemaParser)
            {
                this.schemaParser = schemaParser;
            }

            /// <summary>
            /// Checks if parentYaml refers to another type, either via $base or $type.
            /// If so, returns the properties from that type.
            /// If parentYaml is an object, then checks for ...
            ///   $base is used by TYPES.
            ///   $type is used by paths.
            /// If parentYaml is a string, then the string is the reference type.
            /// </summary>
            /// <param name="schemaId"></param>
            /// <param name="parentYaml"></param>
            /// <returns></returns>
            private async Task<IEnumerable<Property>> GetReferencedProperties(string schemaId, YamlNode parentYaml)
            {
                // $type and $base are functionally the same thing. 
                // they describe where to get properties from.
                // BASE is used by a TYPE. A type can get properties from another type,
                // then manipulate.
                // TYPE is used to specify that something is of a type. But, you can't add
                // more to it.
                // To be semantically accurate, TYPES use BASE and PATHS use TYPE.
                var baseTypeName =
                    parentYaml.AsString("$type") // paths can be assigned to a type.
                    ?? parentYaml.AsString("$base") // types can have bases.
                    ?? parentYaml.AsString();
                if (baseTypeName == null)
                {
                    return new List<Property>();
                }

                var id = NormalizeTypeId(schemaId, baseTypeName);
                var baseType = await schemaParser.GetSchemaType(id);
                return baseType.Properties;
            }

            /// <summary>
            /// Gets the properties defined within the propertiesContainer.
            /// </summary>
            /// <param name="schemaId"></param>
            /// <param name="propertiesContainer"></param>
            /// <returns></returns>
            public async Task<IEnumerable<Property>> GetProperties(string schemaId, YamlNode propertiesContainer)
            {
                // the properties container is the element that contains properties, such as a TYPE or PATH.
                // properties may be defined as an object within the propertiesContainer.
                // or, the container can be a scalar value that is the name of a type that has the properties.
                // so, the container may be a YamlMappingNode, or a YamlScalarNode.
                //
                // propertiesContainer may be:
                //      a YamlMappingNode that has a PropertiesNode.
                //              "paths":
                //                  "/a/b/c":                   <---- propertiesContainer.
                //                      "properties":
                //      
                //      or a YamlScalarNode that points to a type:
                //              "paths":
                //                  "/a/b/c": "base type"       <---- propertiesContainer.
                //
                //      the same applies within types.
                //              "types":
                //                  "kafka":                    <---- propertiesContainer.
                //                      "properties":
                var properties = new List<Property>();

                // the properties defined locally in this object.
                var propertiesYaml = propertiesContainer.AsMap("properties");

                // the secrets short-cut:  "secrets": ["a", "b", "c"]
                var secrets = propertiesContainer.AsStringHashSet("secrets");

                // add properties from the base type.
                properties.AddRange(await GetReferencedProperties(schemaId, propertiesContainer));
                foreach (var p in propertiesYaml)
                {
                    //  Type may be specified 2 ways.
                    //       properties:
                    //          "value": "string"     # shortcut to specify the type name.
                    //          "object":           
                    //             "type": "string"     # long hand. needed when there are other values to set too.
                    //             "is-secret": true    # this can be avoided by using the secrets shortcut.
                    var isObject = p.Value is YamlMappingNode;
                    var typeIdName = isObject
                        ? (string) ((YamlMappingNode) p.Value)["type"]
                        : (string) p.Value;

                    var propertyName = (string) p.Key;
                    var typeId = NormalizeTypeId(schemaId, typeIdName);
                    if (typeId.IsPrimitive)
                    {
                        var isSecret = secrets.Contains(propertyName) || p.Value.AsBoolean("is-secret");
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

                EnsureValuesAreValid("Secrets contains invalid property names.", secrets,
                    properties.Select(p => p.Name).ToHashSet());
                return properties;
            }
        }

        /// <summary>
        /// Given 2 sets, make sure all items in the first set
        /// exist in the second set.
        /// IE: set 1 is a,b,c,d
        ///     set 2 is a,b,c
        /// An exception will be thrown because "d" doesn't
        /// exist in the second set.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="existingNames"></param>
        /// <param name="allowedNames"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void EnsureValuesAreValid(string errorMessage,
            IReadOnlySet<string> existingNames,
            IReadOnlySet<string> allowedNames)
        {
            var bad = existingNames.Except(allowedNames).ToList();
            if (bad.Count == 0)
            {
                return;
            }

            var invalidNames = string.Join(", ", bad);
            var validNames = string.Join(", ", allowedNames);
            var message = errorMessage + "\nInvalid: " + invalidNames + "\nValid: " + validNames;
            throw new InvalidOperationException(message);
        }
    }
}