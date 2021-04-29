using System;
using System.Collections.Generic;
using System.Linq;
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

        public static HashSet<string> ChildAsHashSet(this YamlNode parent, string childName)
        {
            if (parent is YamlMappingNode map)
            {
                return
                    map.Children.ContainsKey(childName)
                        ? ((YamlSequenceNode)map[childName]).Select(i => (string)i).ToHashSet()
                        : new HashSet<string>();
            }

            return new HashSet<string>();
        }
    }
}