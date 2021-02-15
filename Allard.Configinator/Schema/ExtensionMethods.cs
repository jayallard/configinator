using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public static class ExtensionMethods
    {
        public static string StringValue(this YamlNode node, string name)
        {
            return (string) node[name];
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

        public static HashSet<string> ChildAsHashSet(this YamlNode parent, string childName)
        {
            if (parent is YamlMappingNode map)
            {
                return
                    map.Children.ContainsKey(childName)
                        ? null
                        : new HashSet<string>();
            }

            return new HashSet<string>();
        }
    }
}