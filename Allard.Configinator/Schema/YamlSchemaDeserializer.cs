using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public static class YamlSchemaDeserializer
    {
        /// <summary>
        /// Deserializes schema yaml into objects.
        /// </summary>
        /// <param name="docs"></param>
        /// <returns></returns>
        public static IEnumerable<ModelDto.TypeDto> Deserialize(IEnumerable<YamlDocument> docs)
        {
            // the deserializer doesn't apply any logic, it simply deserializes into DTOs
            // and lets the consumer work it out.
            // IE:   OPTIONAL may be specified via the OPTIONAL COLLECTION, or as a setting within a property.
            //       The same is true for SECRETS.
            // The deserializer doesn't need to resolve these things, it just returns the values as they
            // are in the doc.
            return docs
                .Select(d => d.RootNode.AsMap())
                .Where(d => d.AsString("$$doc") == "schema")
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