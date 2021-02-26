using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public static class YamlSchemaDeserializer
    {
        /// <summary>
        ///     Deserializes schema yaml into objects.
        /// </summary>
        /// <param name="docs"></param>
        /// <returns></returns>
        public static IEnumerable<TypeDto> Deserialize(IEnumerable<YamlDocument> docs)
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
                    .Select(type => new TypeDto
                    {
                        Namespace = d.AsRequiredString("namespace"),
                        TypeName = (string) type.Key,
                        BaseTypeName = (string) type.Value.AsString("$base"),
                        Secrets = type.Value.AsStringHashSet("secrets"),
                        Optional = type.Value.AsStringHashSet("optional"),
                        Properties = GetProperties(type.Value.AsMap())
                    })
                )
                .ToList();
        }

        private static IList<PropertyDto> GetProperties(YamlNode propertiesContainer)
        {
            return propertiesContainer
                .AsMap("properties")
                .Select(p =>
                {
                    var isObject = p.Value is YamlMappingNode;
                    var typeName = isObject
                        ? p.Value.AsString("type")
                        : (string) p.Value;

                    var nestedProperties = isObject
                        ? GetProperties(p.Value.AsMap())
                        : new List<PropertyDto>();

                    return new PropertyDto
                    {
                        IsOptional = p.Value.AsBoolean("is-optional"),
                        IsSecret = p.Value.AsBoolean("is-secret"),
                        Secrets = p.Value.AsStringHashSet("secrets"),
                        Optional = p.Value.AsStringHashSet("optional"),
                        PropertyName = (string) p.Key,
                        TypeName = typeName,
                        Properties = nestedProperties
                    };
                }).ToList();
        }
    }
}