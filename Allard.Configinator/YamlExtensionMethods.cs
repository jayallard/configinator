using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Schema;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator
{
    public static class YamlExtensionMethods
    {
        /// <summary>
        ///     Returns the value of a child node as a string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string AsString(this YamlNode node, string name, string defaultValue = null)
        {
            node.EnsureValue(nameof(node));
            name.EnsureValue(nameof(name));

            if (node is YamlMappingNode map)
                return
                    map.Children.ContainsKey(name)
                        ? (string) map[name]
                        : defaultValue;

            return defaultValue;
        }

        /// <summary>
        ///     Returns the node as a boolean.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool AsBoolean(this YamlNode node)
        {
            node.EnsureValue(nameof(node));
            return bool.Parse((string) node);
        }

        /// <summary>
        ///     Returns a child node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static YamlNode Child(this YamlNode parent, string childName)
        {
            return ((YamlMappingNode) parent)[childName];
        }

        /// <summary>
        ///     Returns a child node as a boolean.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool AsBoolean(this YamlNode parent, string childName, bool defaultValue = false)
        {
            if (parent is YamlMappingNode map)
                return
                    map.Children.ContainsKey(childName)
                        ? map.Child(childName).AsBoolean()
                        : defaultValue;

            return defaultValue;
        }

        /// <summary>
        ///     Returns a child node as a YamlMappingNode.
        ///     If the child doesn't exist, returns an empty YamlMappingNode.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static YamlMappingNode AsMap(this YamlNode parent, string childName)
        {
            parent.EnsureValue(nameof(parent));
            childName.EnsureValue(nameof(childName));
            if (parent is YamlMappingNode map)
                return
                    map.Children.ContainsKey(childName)
                        ? (YamlMappingNode) map.Child(childName)
                        : new YamlMappingNode();

            return new YamlMappingNode();
        }

        public static YamlMappingNode AsMap(this YamlNode node)
        {
            return (YamlMappingNode) node;
        }

        /// <summary>
        ///     Returns the node as a string value.
        ///     If the node is not a YamlScalarNode, it returns null.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string AsString(this YamlNode node)
        {
            node.EnsureValue(nameof(node));
            if (node is YamlScalarNode scalarNode) return (string) scalarNode;
            return null;
        }

        public static string AsRequiredString(this YamlNode node, string nodeName)
        {
            var value = node.AsString(nodeName);
            return string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException("Required node is missing or does not have a value: " + nodeName)
                : value;
        }

        /// <summary>
        ///     Returns child nodes as a set of strings.
        ///     If the node isn't a YamlMappingNode, it returns
        ///     an empty set. If it is a YamlMappingNode, then
        ///     the value must be a list of strings, or it will
        ///     throw an exception when parsing.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static HashSet<string> AsStringHashSet(this YamlNode parent, string childName)
        {
            parent.EnsureValue(nameof(parent));
            childName.EnsureValue(nameof(childName));
            if (parent is YamlMappingNode map)
                return
                    map.Children.ContainsKey(childName)
                        ? ((YamlSequenceNode) map[childName]).Select(i => (string) i).ToHashSet()
                        : new HashSet<string>();

            return new HashSet<string>();
        }

        /// <summary>
        ///     Emit a collection of properties to a json writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="properties"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void WriteProperties(Utf8JsonWriter writer, IEnumerable<Property> properties)
        {
            writer.EnsureValue(nameof(writer));
            var props = properties.EnsureValue(nameof(properties)).ToList();
            foreach (var property in props)
                switch (property)
                {
                    case PropertyPrimitive prim:
                        // TODO: support other types as they come online
                        writer.WriteString(prim.Name, "string");
                        continue;
                    case PropertyGroup group:
                        writer.WriteStartObject(group.Name);
                        WriteProperties(writer, group.Properties);
                        writer.WriteEndObject();
                        continue;
                    default:
                        throw new InvalidOperationException("Unknown property type");
                }
        }
    }
}