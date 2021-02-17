using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public static class ExtensionMethods
    {
        public static string ChildAsString(this YamlNode node, string name, string defaultValue = null)
        {
            if (node is YamlMappingNode map)
            {
                return
                    map.Children.ContainsKey(name)
                        ? (string) map[name]
                        : defaultValue;
            }

            return defaultValue;
        }

        public static bool AsBoolean(this YamlNode node)
        {
            return bool.Parse((string) node);
        }

        public static YamlNode Child(this YamlNode parent, string childName)
        {
            return ((YamlMappingNode) parent)[childName];
        }

        public static bool ChildAsBoolean(this YamlNode parent, string childName, bool defaultValue = false)
        {
            if (parent is YamlMappingNode map)
            {
                return
                    map.Children.ContainsKey(childName)
                        ? map.Child(childName).AsBoolean()
                        : defaultValue;
            }

            return defaultValue;
        }

        public static YamlMappingNode ChildAsMap(this YamlNode parent, string childName)
        {
            if (parent is YamlMappingNode map)
            {
                return
                    map.Children.ContainsKey(childName)
                        ? (YamlMappingNode) map.Child(childName)
                        : new YamlMappingNode();
            }

            return new YamlMappingNode();
        }

        public static IReadOnlySet<string> ChildNames(this YamlNode node)
        {
            if (node is YamlMappingNode mapping)
            {
                return mapping.Children.Select(p => (string) p.Key).ToHashSet();
            }

            return new HashSet<string>();
        }

        public static string CurrentAsString(this YamlNode current)
        {
            if (current is YamlScalarNode scalarNode)
            {
                return (string) scalarNode;
            }

            return null;
        }

        public static HashSet<string> ChildAsHashSet(this YamlNode parent, string childName)
        {
            if (parent is YamlMappingNode map)
            {
                return
                    map.Children.ContainsKey(childName)
                        ? ((YamlSequenceNode) map[childName]).Select(i => (string) i).ToHashSet()
                        : new HashSet<string>();
            }

            return new HashSet<string>();
        }

        public static IEnumerable<JsonDocument> ToSampleJson(this ConfigurationSchema schema)
        {
            return schema
                .Paths
                .Select(p =>
                {
                    var options = new JsonWriterOptions {Indented = true};
                    using var stream = new MemoryStream();
                    using var jsonWriter = new Utf8JsonWriter(stream, options);
                    jsonWriter.WriteStartObject();
                    WriteProperties(jsonWriter, p.Properties);
                    jsonWriter.WriteEndObject();
                    jsonWriter.Flush();
                    var json = Encoding.UTF8.GetString(stream.ToArray());
                    return JsonDocument.Parse(json);
                });
        }

        private static void WriteProperties(Utf8JsonWriter writer, IEnumerable<Property> properties)
        {
            foreach (var property in properties)
            {
                switch (property)
                {
                    case PropertyPrimitive prim:
                        // TODO: support other types as they come online
                        writer.WriteString(prim.Name, "string");
                        continue;
                    case PropertyGroup group:
                        writer.WriteStartObject(@group.Name);
                        WriteProperties(writer, @group.Properties);
                        writer.WriteEndObject();
                        continue;
                    default:
                        throw new InvalidOperationException("Unknown property type");
                }
            }
        }
    }
}