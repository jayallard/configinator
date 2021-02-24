using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public class YamlSchemaDeserializer
    {
        public static IList<ModelDto.TypeDto> Deserialize(IEnumerable<YamlStream> docs)
        {
            return new YamlSchemaDeserializer(docs).Deserialize();
        }


        private readonly List<YamlMappingNode> docs;

        public YamlSchemaDeserializer(IEnumerable<YamlStream> docs)
        {
            this.docs = docs
                .SelectMany(d => d.Documents)
                .Select(d => d.RootNode.AsMap())
                .Where(d => d.AsString("$$doc") == "schema")
                .ToList();
        }

        private List<ModelDto.TypeDto> Deserialize()
        {
            return docs
                .SelectMany(d => d.AsMap("types")
                    .Select(type => new ModelDto.TypeDto
                    {
                        Namespace = d.AsRequiredString("namespace"),
                        TypeName = (string) type.Key,
                        BaseTypeName = (string) type.Value.AsString("$base"),
                        Secrets = type.Value.AsStringHashSet("secrets"),
                        Optional = type.Value.AsStringHashSet("optional"),
                        Properties = type
                            .Value
                            .AsMap("properties")
                            .Select(p =>
                            {
                                var isObject = p.Value is YamlMappingNode;
                                var typeName = isObject
                                    ? p.Value.AsString("type")
                                    : (string) p.Value;

                                return new ModelDto.PropertyDto
                                {
                                    IsOptional = p.Value.AsBoolean("is-optional"),
                                    IsSecret = p.Value.AsBoolean("is-secret"),
                                    PropertyName = (string) p.Key,
                                    TypeName = typeName
                                };
                            }).ToList()
                    })
                )
                .ToList();
        }
    }
}