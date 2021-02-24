using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

// TODO: this needs one more rewrite. it's currently based on TYPE.
// we need it to be based on NAMESPACE instead.

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     Converts Yaml to a
    /// </summary>
    public class SchemaService : ISchemaService
    {
        /// <summary>
        ///     The node names that are valid within a type node
        ///     "types":
        ///     "my-type":
        ///     these nodes
        /// </summary>
        private readonly HashSet<string>
            allowedTypeNodeName = new(new[] {"$base", "properties", "secrets", "optional"});

        /// <summary>
        ///     Retrieves the Yaml.
        /// </summary>
        private readonly ISchemaRepository repository;

        /// <summary>
        ///     Converts YamlProperties to property objects.
        /// </summary>
        private readonly PropertyParser propertyParser;

        /// <summary>
        ///     Stores the schema types per type id.
        /// </summary>
        private readonly ConcurrentDictionary<SchemaTypeId, ObjectSchemaType> schemaTypes = new();

        /// <summary>
        ///     Stores the YAML per schema id.
        /// </summary>
        private readonly ConcurrentDictionary<string, YamlMappingNode> sourceYaml = new();

        /// <summary>
        ///     Initializes an instance of the SchemaParser class.
        /// </summary>
        /// <param name="repository"></param>
        public SchemaService(ISchemaRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            propertyParser = new PropertyParser(this);
        }

        /// <summary>
        ///     Converts a type id to a standard format.
        ///     If the id is "./type", it is converted to "relativeSchemaId/type".
        ///     If the id starts with "/x", it is converted to "primitive-types/x",
        ///     where x can be any primitive type.
        /// </summary>
        /// <param name="relativeSchemaId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        private static SchemaTypeId NormalizeTypeId(string relativeSchemaId, string typeId)
        {
            return typeId.StartsWith("./")
                // if references this schema, then change the ./ to this schema id.
                // IE:  "./typeId" becomes "currentSchemaId/typeId"
                ? new SchemaTypeId(relativeSchemaId + "/" + typeId.Substring(2))
                : new SchemaTypeId(typeId);
        }

        public async Task<ObjectSchemaType> GetSchemaTypeAsync(string typeId)
        {
            typeId = string.IsNullOrWhiteSpace(typeId)
                ? throw new ArgumentNullException(typeId)
                : typeId;
            return await GetSchemaTypeAsync(new SchemaTypeId(typeId));
        }

        /// <summary>
        ///     Returns the schema type of the given id.
        /// </summary>
        /// <param name="typeId">The id of the schema type to return.</param>
        /// <returns></returns>
        private async Task<ObjectSchemaType> GetSchemaTypeAsync(SchemaTypeId typeId)
        {
            if (schemaTypes.ContainsKey(typeId)) return schemaTypes[typeId];

            var type = await BuildTypeAsync(typeId);
            schemaTypes[typeId] = type;
            return type;
        }

        /// <summary>
        ///     Build a schema type from yaml.
        /// </summary>
        /// <param name="schemaTypeId"></param>
        /// <returns></returns>
        private async Task<ObjectSchemaType> BuildTypeAsync(SchemaTypeId schemaTypeId)
        {
            var sourceSchema = await GetYamlAsync(schemaTypeId.NameSpace);
            var typesYaml = (YamlMappingNode) sourceSchema["types"];
            var (_, value) = typesYaml.Single(t => (string) t.Key == schemaTypeId.TypeId);
            EnsureValuesAreValid("TYPE node has invalid children.", value.ChildNames(), allowedTypeNodeName);
            var props = new List<Property>();

            // from the local type
            props.AddRange(
                await propertyParser.GetProperties(schemaTypeId.NameSpace, (YamlMappingNode) value));
            return new ObjectSchemaType(schemaTypeId, props.AsReadOnly());
        }

        /// <summary>
        ///     Get the Yaml for the schema.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<YamlMappingNode> GetYamlAsync(string schemaId)
        {
            if (sourceYaml.ContainsKey(schemaId)) return sourceYaml[schemaId];
            var source = await repository.GetSchemaYaml(schemaId);

            // make sure the id from the source matches the requested id.
            var nameSpace = (string) source["namespace"];
            if (schemaId != nameSpace)
                throw new InvalidOperationException(
                    $"Namespace mismatch. Namespace={schemaId}, Namespace in File={nameSpace}");

            sourceYaml[schemaId] = source;
            return sourceYaml[schemaId];
        }

        /// <summary>
        ///     Given 2 sets, make sure all items in the first set
        ///     exist in the second set.
        ///     IE: set 1 is a,b,c,d
        ///     set 2 is a,b,c
        ///     An exception will be thrown because "d" doesn't
        ///     exist in the second set.
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
            if (bad.Count == 0) return;

            var invalidNames = string.Join(", ", bad);
            var validNames = string.Join(", ", allowedNames);
            var message = errorMessage + "\nInvalid: " + invalidNames + "\nValid: " + validNames;
            throw new InvalidOperationException(message);
        }

        /// <summary>
        ///     Parses properties for paths and for types.
        ///     It collects properties from base type
        ///     and reference types, etc. The whole gambit.
        /// </summary>
        private class PropertyParser
        {
            /// <summary>
            ///     Used to retrieve other types.
            /// </summary>
            private readonly SchemaService schemaService;

            /// <summary>
            ///     Initializes a new instance of the PropertyParser class.
            /// </summary>
            /// <param name="schemaService"></param>
            public PropertyParser(SchemaService schemaService)
            {
                this.schemaService = schemaService;
            }

            /// <summary>
            /// </summary>
            /// <param name="schemaId"></param>
            /// <param name="parentYaml"></param>
            /// <returns></returns>
            private async Task<IEnumerable<Property>> GetReferencedProperties(string schemaId, YamlNode parentYaml)
            {
                var baseTypeName =
                    parentYaml.AsString("$base") // types can have bases.
                    ?? parentYaml.AsString();
                if (baseTypeName == null) return new List<Property>();

                var id = NormalizeTypeId(schemaId, baseTypeName);
                var baseType = await schemaService.GetSchemaTypeAsync(id);
                return baseType.Properties;
            }

            /// <summary>
            ///     Gets the properties defined within the propertiesContainer.
            /// </summary>
            /// <param name="relativeSchemaId"></param>
            /// <param name="propertiesContainer"></param>
            /// <returns></returns>
            internal async Task<IEnumerable<Property>> GetProperties(string relativeSchemaId,
                YamlNode propertiesContainer)
            {
                var properties = new List<Property>();

                // the properties defined locally in this object.
                var propertiesYaml = propertiesContainer.AsMap("properties");

                // the secrets short-cut:  "secrets": ["a", "b", "c"]
                var secretProperties = propertiesContainer.AsStringHashSet("secrets");

                // the optional short-cut: "optional": ["a", "b", "c"]
                var optionalProperties = propertiesContainer.AsStringHashSet("optional");

                // add properties from the base type.
                properties.AddRange(await GetReferencedProperties(relativeSchemaId, propertiesContainer));
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
                    var typeId = NormalizeTypeId(relativeSchemaId, typeIdName);
                    var isOptional = optionalProperties.Contains(propertyName) || p.Value.AsBoolean("is-optional");
                    if (typeId.IsPrimitive)
                    {
                        var isSecret = secretProperties.Contains(propertyName) || p.Value.AsBoolean("is-secret");
                        properties.Add(new PropertyPrimitive(propertyName, typeId, isSecret, isOptional));
                        continue;
                    }

                    // get the properties for the type.
                    // append additional properties added at
                    // the schema level.
                    var propertiesForType = new List<Property>();
                    var type = await schemaService.GetSchemaTypeAsync(typeId);
                    propertiesForType.AddRange(type.Properties);
                    properties.Add(new PropertyGroup(propertyName, typeId, isOptional, propertiesForType.AsReadOnly()));
                }

                EnsureValuesAreValid("Secrets contains invalid property names.", secretProperties,
                    properties.Select(p => p.Name).ToHashSet());
                return properties;
            }
        }
    }
}