using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ConfigurationManagement.Code.Schema
{
    public class SchemaParser
    {
        private readonly Dictionary<string, YamlNode> sourceYaml = new();
        private readonly Dictionary<string, ConfigurationSchema> schemas = new();
        private readonly Dictionary<string, SchemaType> schemaTypes = new();

        private readonly ISchemaRepository repository;

        public SchemaParser(ISchemaRepository repository)
        {
            this.repository = repository;
        }

        private async Task<YamlNode> GetSource(string id)
        {
            if (sourceYaml.ContainsKey(id))
            {
                return sourceYaml[id];
            }

            sourceYaml[id] = await repository.GetSchema(id);
            return sourceYaml[id];
        }

        public async Task<ConfigurationSchema> GetSchema(string id)
        {
            if (schemas.ContainsKey(id))
            {
                return schemas[id];
            }

            var source = await GetSource(id);
            var actualId = (string) source["id"];
            if (id != actualId)
            {
                throw new InvalidOperationException("The schema id doesn't match.");
            }

            var pathNodes = (YamlMappingNode) source["paths"];
            var paths = new List<PathNode>();
            foreach (var pathNode in pathNodes)
            {
                var path = (string) pathNode.Key;
                var objectNode = (YamlMappingNode) pathNode.Value;
                var properties = await GetProperties(objectNode, actualId);
                paths.Add(new PathNode {Path = path, Properties = properties});
            }

            return new ConfigurationSchema {Paths = paths.AsReadOnly(), Id = actualId};
        }

        private async Task<List<Property>> GetProperties(YamlMappingNode objectNode, string currentSchemaId)
        {
            var baseType = objectNode.Children.ContainsKey("$base")
                ? (string) objectNode["$base"]
                : null;

            var properties = new List<Property>();
            if (baseType != null)
            {
                var type = await GetSchemaType(baseType, currentSchemaId);
                properties.AddRange(type.Properties);
            }

            if (objectNode.Children.ContainsKey("properties"))
            {
                var propertiesNode = (YamlMappingNode) objectNode["properties"];
                foreach (var propertyNode in propertiesNode)
                {
                    var property = await CreateProperty(propertyNode, currentSchemaId);
                    properties.Add(property);
                }
            }

            return properties;
        }

        private async Task<SchemaType> GetSchemaType(string fullTypeId, string currentSchemaId)
        {
            if (fullTypeId.StartsWith("./"))
            {
                fullTypeId = currentSchemaId + "/" + fullTypeId.Substring(2);
            }

            if (schemaTypes.ContainsKey(fullTypeId))
            {
                return schemaTypes[fullTypeId];
            }

            // "schemaId/typeId"
            var ownerSchemaId =
                fullTypeId.Contains("/")
                    ? fullTypeId.Substring(0, fullTypeId.IndexOf("/", StringComparison.Ordinal))
                    : null;

            // just "typeId"
            var relativeTypeName =
                fullTypeId.Contains("/")
                    ? fullTypeId.Substring(fullTypeId.IndexOf("/") + 1)
                    : fullTypeId;

            var isPrimitive = ownerSchemaId == null;
            if (isPrimitive)
            {
                var property = relativeTypeName switch
                {
                    "string" => new PropertyValue {Name = "todo", IsSecret = false},
                    _ => throw new InvalidOperationException("unknown primitive type: " + relativeTypeName)
                };

                var props = new List<Property> {property}.AsReadOnly();
                return new SchemaType {Properties = props, TypeId = relativeTypeName};
            }

            var schemaYaml = await GetSource(ownerSchemaId);
            var typesNode = (YamlMappingNode) schemaYaml["types"];
            if (typesNode == null)
            {
                throw new InvalidOperationException("Type doesn't exist: " + fullTypeId);
            }

            var objectNode = (YamlMappingNode) typesNode[relativeTypeName];
            if (objectNode == null)
            {
                throw new InvalidOperationException("Type doesn't exist: " + fullTypeId);
            }

            var properties = await GetProperties(objectNode, ownerSchemaId);
            return new SchemaType {Properties = properties.AsReadOnly(), TypeId = fullTypeId};
        }

        private async Task<Property> CreateProperty(
            KeyValuePair<YamlNode, YamlNode> propertyNode,
            string currentSchemaId)
        {
            var propertyName = (string) propertyNode.Key;
            var isObject = propertyNode.Value is YamlMappingNode;
            var typeName =
                isObject
                    ? (string) ((YamlMappingNode) propertyNode.Value)["type"]
                    : (string) propertyNode.Value;

            /*switch (typeName)
            {
                case "string":
                    return new PropertyValue {Name = propertyName, IsSecret = false};
            }*/

            var type = await GetSchemaType(typeName, currentSchemaId);
            var additionalProperties = isObject
                ? await GetProperties((YamlMappingNode) propertyNode.Value, currentSchemaId)
                : new List<Property>();

            var props = new List<Property>();
            props.AddRange(type.Properties);
            props.AddRange(additionalProperties);
            return new PropertyGroup {Name = propertyName, Properties = props.AsReadOnly()};
        }
    }
}