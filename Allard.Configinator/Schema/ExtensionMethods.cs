using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the value of a child node as a string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string AsString(this YamlNode node, string name, string defaultValue = null)
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

        /// <summary>
        /// Returns the node as a boolean.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool AsBoolean(this YamlNode node)
        {
            return bool.Parse((string) node);
        }

        /// <summary>
        /// Returns a child node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static YamlNode Child(this YamlNode parent, string childName)
        {
            return ((YamlMappingNode) parent)[childName];
        }

        /// <summary>
        /// Returns a child node as a boolean.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool AsBoolean(this YamlNode parent, string childName, bool defaultValue = false)
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

        /// <summary>
        /// Returns a child node as a YamlMappingNode.
        /// If the child doesn't exist, returns an empty YamlMappingNode.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static YamlMappingNode AsMap(this YamlNode parent, string childName)
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

        public static YamlMappingNode AsMap(this YamlNode node)
        {
            return (YamlMappingNode) node;
        }

        /// <summary>
        /// Returns a set of the name of the children of the node.
        /// If the node is not a YamlMappingNode, it returns an empty set.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IReadOnlySet<string> ChildNames(this YamlNode node)
        {
            if (node is YamlMappingNode mapping)
            {
                return mapping.Children.Select(p => (string) p.Key).ToHashSet();
            }

            return new HashSet<string>();
        }

        /// <summary>
        /// Returns the node as a string value.
        /// If the node is not a YamlScalarNode, it returns null.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string AsString(this YamlNode node)
        {
            if (node is YamlScalarNode scalarNode)
            {
                return (string) scalarNode;
            }

            return null;
        }

        /// <summary>
        /// Returns child nodes as a set of strings.
        /// If the node isn't a YamlMappingNode, it returns
        /// an empty set. If it is a YamlMappingNode, then
        /// the value must be a list of strings, or it will
        /// throw an exception when parsing.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static HashSet<string> AsStringHashSet(this YamlNode parent, string childName)
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

        /*
        /// <summary>
        /// Converts a schema to sample json documents.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
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
        }*/

        /// <summary>
        /// Emit a collection of properties to a json writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="properties"></param>
        /// <exception cref="InvalidOperationException"></exception>
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