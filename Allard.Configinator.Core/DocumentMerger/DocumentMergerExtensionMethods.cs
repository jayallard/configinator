using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public static class DocumentMergerExtensionMethods
    {
        public static bool AssignedExplicitValue(this Transition transition) =>
            transition == Transition.Set
            || transition == Transition.SetToSameValue;

        public static string ToJsonString(this IEnumerable<MergedProperty> properties)
        {
            var props = properties
                .EnsureValue(nameof(properties))
                .OrderBy(p => p.Path.Length)
                .ThenBy(p => p.Path);

            // need to convert to a dictionary,
            // then to json. lame.
            // system.text.json is immutable, so can't 
            // modify things along the way.
            // there must be a better way, but one
            // thing at a time. 
            // for map conversion, this isn't optimized.
            // can definitely group on paths, etc.
            // this one crawls from the beginning each time...
            // very repetitive thus inefficient.
            // quick and dirty for now.
            var map = new ConcurrentDictionary<string, object>();
            foreach (var p in props)
            {
                var currentObject = map;
                var parts = p.Path.Split("/");
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    currentObject =
                        (ConcurrentDictionary<string, object>) currentObject.GetOrAdd(parts[i],
                            new ConcurrentDictionary<string, object>());
                }

                currentObject[parts[parts.Length-1]] = p.Property.Value;
            }

            // now convert the map to a json string.
            using var output = new MemoryStream();
            using var writer = new Utf8JsonWriter(output);
            writer.WriteStartObject();
            WriteObject(writer, map);
            writer.WriteEndObject();
            writer.Flush();
            output.Position = 0;
            using var streamReader = new StreamReader(output);
            return streamReader.ReadToEnd();
        }

        private static void WriteObject(Utf8JsonWriter writer, ConcurrentDictionary<string, object> map)
        {
            foreach (var kv in map)
            {
                if (kv.Value is ConcurrentDictionary<string, object> obj)
                {
                    writer.WriteStartObject(kv.Key);
                    //WriteObject(writer, obj);
                    writer.WriteEndObject();
                    continue;
                }
                //WriteProperties(writer, map);
            }
        }
        private static void WriteProperties(Utf8JsonWriter writer, ConcurrentDictionary<string, object> map)
        {
            foreach (var kv in map)
            {
                switch (kv.Value)
                {
                    case ConcurrentDictionary<string, object> nestedObj:

                        continue;
                    case null:
                        writer.WriteNull(kv.Key);
                        continue;
                    case string stringValue:
                        writer.WriteString(kv.Key, stringValue);
                        break;
                    default:
                        throw new InvalidOperationException("Unhandled data type: " + kv.Value.GetType().FullName);
                }
            }
        }
    }
}