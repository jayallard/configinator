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
        private readonly Dictionary<string, YamlMappingNode> sourceYaml = new();
        private readonly Dictionary<string, ConfigurationSchema> schemas = new();
        private readonly Dictionary<SchemaTypeId, ObjectSchemaType> schemaTypes = new();
        private readonly ISchemaRepository repository;
        private readonly PropertyParser propertyParser;

        public SchemaParser(ISchemaRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            propertyParser = new PropertyParser(this);
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
            var pathNodes = (YamlMappingNode) source["paths"];
            var paths = new List<PathNode>();
            foreach (var p in pathNodes)
            {
                var properties = await propertyParser
                    .GetProperties(schemaId, (YamlMappingNode) p.Value);
                paths.Add(new PathNode
                {
                    Properties = properties.ToList(),
                    Path = (string) p.Key
                });
            }

            return new ConfigurationSchema
            {
                Id = schemaId,
                Paths = paths.AsReadOnly()
            };
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
            var typesYanl = (YamlMappingNode) sourceSchema["types"];
            var typeYaml = typesYanl.Single(t => (string) t.Key == schemaTypeId.TypeId);
            var props = new List<Property>();

            // from the base
            var baseTypeName = typeYaml.Value.ChildAsString("$base");
            if (baseTypeName != null)
            {
                var baseTypeId = NormalizeTypeId(schemaTypeId.SchemaId, baseTypeName);
                var baseType = await GetType(baseTypeId);
                props.AddRange(baseType.Properties);
            }

            // from the local type
            props.AddRange(await propertyParser.GetProperties(schemaTypeId.SchemaId, (YamlMappingNode) typeYaml.Value));
            return new ObjectSchemaType(schemaTypeId, props);
        }

        private async Task<YamlMappingNode> GetSource(string schemaId)
        {
            if (sourceYaml.ContainsKey(schemaId))
            {
                return sourceYaml[schemaId];
            }

            var source = await repository.GetSchema(schemaId);
            var sourceId = (string) source["id"];
            if (schemaId != sourceId)
            {
                throw new InvalidOperationException($"Schema id mismatch. Schema Id={schemaId}, Id in File={sourceId}");
            }

            sourceYaml[schemaId] = source;
            return sourceYaml[schemaId];
        }

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

        /*
        private interface IPropertiesYaml
        {
            SchemaYaml Owner { get; }
            YamlMappingNode Yaml { get; }
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

        [DebuggerDisplay("{SchemaTypeId}")]
        private record TypeYaml : IPropertiesYaml
        {
            public SchemaYaml Owner { get; }
            public YamlMappingNode Yaml { get; }
            public SchemaTypeId SchemaTypeId { get; }
            public SchemaTypeId BaseType { get; }

            public TypeYaml(SchemaYaml owner, KeyValuePair<YamlNode, YamlNode> typeYaml)
            {
                var value = (YamlMappingNode) typeYaml.Value;
                Owner = owner;
                Yaml = (YamlMappingNode) value["properties"];
                SchemaTypeId = new SchemaTypeId(owner.SchemaId + "/" + (string) typeYaml.Key);
                BaseType = value.Children.ContainsKey("$base")
                    //? new SchemaTypeId((string) value["$base"])
                    ? NormalizeTypeId(owner.SchemaId, (string) value["$base"])
                    : null;
            }
        }*/

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
                    var typeId = NormalizeTypeId(schemaId, typeIdName);
                    if (typeId.IsPrimitive)
                    {
                        properties.Add(new PropertyPrimitive
                        {
                            Name = (string) p.Key,
                            IsSecret = p.Value.ChildAsBoolean("is-secret"),
                            TypeId = typeId
                        });

                        continue;
                    }

                    // get the properties for the type.
                    // append additional propertiees added at
                    // the schema leel.
                    var propertiesForType = new List<Property>();
                    var type = await schemaParser.GetType(typeId);
                    propertiesForType.AddRange(type.Properties);
                    properties.Add(new PropertyGroup
                    {
                        Properties = propertiesForType.AsReadOnly(),
                        Name = (string) p.Key,
                        TypeId = type.SchemaTypeId
                    });
                }

                return properties;
            }
        }

        /*[DebuggerDisplay("{Path}")]
        private record PathYaml : IPropertiesYaml
        {
            public SchemaYaml Owner { get; }
            
            public YamlMappingNode Yaml { get; }
            public string Path { get; }

            public PathYaml(SchemaYaml owner, KeyValuePair<YamlNode, YamlNode> pathYaml)
            {
                Owner = owner;
                Path = (string) pathYaml.Key;
                Yaml = (YamlMappingNode) pathYaml.Value["properties"];
            }
        }*/
    }
}