using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                var propertyNodes = (YamlMappingNode) pathNode.Value["properties"];
                var properties = await GetProperties(propertyNodes, id);
                paths.Add(new PathNode {Path = path, Properties = properties});
            }

            return new ConfigurationSchema {Paths = paths.AsReadOnly(), Id = actualId};
        }

        private async Task<List<Property>> GetProperties(YamlMappingNode propertyNodes, string currentSchemaId)
        {
            var properties = new List<Property>();
            foreach (var propertyNode in propertyNodes)
            {
                var property = await CreateProperty(propertyNode, currentSchemaId);
                properties.Add(property);
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

            if (ownerSchemaId == null)
            {
                // todo: primitive
                return null;
            }

            var schemaYaml = await GetSource(ownerSchemaId);
            var typesNode = (YamlMappingNode) schemaYaml["types"];
            if (typesNode == null)
            {
                throw new InvalidOperationException("Type doesn't exist: " + fullTypeId);
            }

            var typeNode = (YamlMappingNode) typesNode[relativeTypeName];
            if (typeNode == null)
            {
                throw new InvalidOperationException("Type doesn't exist: " + fullTypeId);
            }

            var propertyNodes = (YamlMappingNode) typeNode["properties"];
            var properties = await GetProperties(propertyNodes, currentSchemaId);
            return new SchemaType {Properties = properties.AsReadOnly(), TypeId = fullTypeId};
        }

        private async Task<Property> CreateProperty(
            KeyValuePair<YamlNode, YamlNode> propertyNode,
            string currentSchemaId)
        {
            var propertyName = (string) propertyNode.Key;
            if (propertyNode.Value is YamlMappingNode objectNode)
            {
                // handle and object structure.
                // "name":
                //    "type": "whatever"
                //    "properties":         (optional)
                //       ... additional properties
                return null;
            }

            // short hand property
            // "name": "type"
            var typeName = (string) propertyNode.Value;
            switch (typeName)
            {
                case "string":
                    return new PropertyValue {Name = propertyName, IsSecret = false};
            }

            var type = await GetSchemaType(typeName, currentSchemaId);
            return new PropertyGroup {Name = propertyName, Properties = type.Properties};
        }
    }
}