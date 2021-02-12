using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ConfigurationManagement.Code.Schema
{
    public class SchemaParser2
    {
        private readonly Dictionary<string, SchemaYaml> sourceYaml = new();
        private readonly Dictionary<string, ConfigurationSchema> schemas = new();
        private readonly Dictionary<SchemaTypeId, ObjectSchemaType> schemaTypes = new();
        private readonly ISchemaRepository repository;

        public SchemaParser2(ISchemaRepository repository)
        {
            this.repository = repository;
        }

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

        private async Task<ConfigurationSchema> BuildSchema(string schemaId)
        {
            var source = await GetSource(schemaId);
            var paths = new List<PathNode>();
            foreach (var p in source.Paths)
            {
                var properties = await GetProperties(p);
                paths.Add(new PathNode
                {
                    Properties = properties,
                    Path = p.Path
                });
            }

            return new ConfigurationSchema
            {
                Id = schemaId,
                Paths = paths.AsReadOnly()
            };
        }

        private async Task<List<Property>> GetProperties(IPropertiesYaml path)
        {
            var properties = new List<Property>();
            foreach (var p in path.PropertiesYaml)
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
                var typeId = NormalizeTypeId(path.Owner.SchemaId, typeIdName);
                if (typeId.IsPrimitive)
                {
                    properties.Add(new PropertyPrimitive
                    {
                        Name = (string) p.Key,
                        IsSecret = false,
                        UnderlyingType = (string) p.Value
                    });

                    continue;
                }

                var type = await GetType(typeId);
                properties.Add(new PropertyGroup
                {
                    Properties = type.Properties,
                    Name = (string) p.Key
                });
            }

            return properties;
        }

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

        private async Task<ObjectSchemaType> GetType(SchemaTypeId typeId)
        {
            if (schemaTypes.ContainsKey(typeId))
            {
                return schemaTypes[typeId];
            }

            var type = await BuildType(typeId);
            schemaTypes[typeId] = type;
            return type;
        }

        private async Task<ObjectSchemaType> BuildType(SchemaTypeId schemaTypeId)
        {
            var sourceSchema = await GetSource(schemaTypeId.SchemaId);
            var typeYaml = sourceSchema.Types.SingleOrDefault(t => t.SchemaTypeId == schemaTypeId);
            var properties = await GetProperties(typeYaml);
            return new ObjectSchemaType(schemaTypeId, properties);
        }

        private async Task<SchemaYaml> GetSource(string schemaId)
        {
            if (sourceYaml.ContainsKey(schemaId))
            {
                return sourceYaml[schemaId];
            }

            var source = await repository.GetSchema(schemaId);

            var sourceId = (string) source["id"];
            if (schemaId != sourceId)
            {
                throw new InvalidOperationException("schema id mismatch");
            }

            sourceYaml[schemaId] = new SchemaYaml((YamlMappingNode) source);
            return sourceYaml[schemaId];
        }

        /*[DebuggerDisplay("{SchemaTypeId}")]
        private abstract record SchemaType
        {
            public SchemaTypeId SchemaTypeId { get; }

            protected SchemaType(SchemaTypeId schemaTypeId)
            {
                SchemaTypeId = schemaTypeId;
            }
        }*/

        /*
        [DebuggerDisplay("{SchemaTypeId} - {PrimitiveType}")]
        private record PrimitiveSchemaType : SchemaType
        {
            public PrimitiveSchemaType(SchemaTypeId typeId, string primitiveType) : base(typeId)
            {
                PrimitiveType = primitiveType;
            }

            public string PrimitiveType { get; }
        }*/

        [DebuggerDisplay("{SchemaTypeId}")]
        private record ObjectSchemaType
        {
            public SchemaTypeId SchemaTypeId { get; }

            public ReadOnlyCollection<Property> Properties { get; }

            public ObjectSchemaType(
                SchemaTypeId schemaTypeId,
                List<Property> properties)
            {
                SchemaTypeId = schemaTypeId;
                Properties = properties.AsReadOnly();
            }
        }

        [DebuggerDisplay("{FullId}")]
        private record SchemaTypeId
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

        [DebuggerDisplay("{SchemaId}")]
        private record SchemaYaml
        {
            public ReadOnlyCollection<TypeYaml> Types { get; }
            public string SchemaId { get; }

            public ReadOnlyCollection<PathYaml> Paths { get; }

            public SchemaYaml(YamlMappingNode schemaYaml)
            {
                SchemaId = (string) schemaYaml["id"];
                Paths = ConvertNodes(schemaYaml, "paths", p => new PathYaml(this, p));
                Types = ConvertNodes(schemaYaml, "types", p => new TypeYaml(this, p));
            }

            private static ReadOnlyCollection<T> ConvertNodes<T>(
                YamlMappingNode schemaYaml,
                string nodeName,
                Func<KeyValuePair<YamlNode, YamlNode>, T> convertMethod)
            {
                if (!schemaYaml.Children.ContainsKey(nodeName))
                {
                    return new List<T>().AsReadOnly();
                }

                return ((YamlMappingNode) schemaYaml[nodeName])
                    .Select(convertMethod)
                    .ToList()
                    .AsReadOnly();
            }
        }

        private interface IPropertiesYaml
        {
            SchemaYaml Owner { get; }
            ReadOnlyCollection<KeyValuePair<YamlNode, YamlNode>> PropertiesYaml { get; }
        }

        [DebuggerDisplay("{SchemaTypeId}")]
        private record TypeYaml : IPropertiesYaml
        {
            public SchemaYaml Owner { get; }
            public ReadOnlyCollection<KeyValuePair<YamlNode, YamlNode>> PropertiesYaml { get; }
            public SchemaTypeId SchemaTypeId { get; }

            public TypeYaml(SchemaYaml owner, KeyValuePair<YamlNode, YamlNode> typeYaml)
            {
                Owner = owner;
                PropertiesYaml = ((YamlMappingNode) typeYaml.Value["properties"])
                    .Children
                    .ToList()
                    .AsReadOnly();
                SchemaTypeId = new SchemaTypeId(owner.SchemaId + "/" + (string) typeYaml.Key);
            }
        }

        [DebuggerDisplay("{Path}")]
        private record PathYaml : IPropertiesYaml
        {
            public SchemaYaml Owner { get; }
            public string Path { get; }
            public ReadOnlyCollection<KeyValuePair<YamlNode, YamlNode>> PropertiesYaml { get; }

            public PathYaml(SchemaYaml owner, KeyValuePair<YamlNode, YamlNode> pathYaml)
            {
                Owner = owner;
                Path = (string) pathYaml.Key;
                PropertiesYaml = ((YamlMappingNode) pathYaml.Value["properties"])
                    .Children
                    .ToList()
                    .AsReadOnly();
            }
        }
    }
}