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
        /// <summary>
        ///     Returns true if the transition is any variation
        ///     of SET.
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        public static bool IsSet(this Transition transition)
        {
            return transition == Transition.Set
                   || transition == Transition.SetToSameValue;
        }

        /// <summary>
        ///     Convert the results of a document merge to a JSON string.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
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
            // ---------------------------------------------------
            // convert to json
            // ---------------------------------------------------
            var map = new ConcurrentDictionary<string, object>();
            foreach (var p in props)
            {
                var currentObject = map;
                var parts = p.Path.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                for (var i = 0; i < parts.Length - 1; i++)
                    currentObject =
                        (ConcurrentDictionary<string, object>) currentObject.GetOrAdd(parts[i],
                            new ConcurrentDictionary<string, object>());

                currentObject[parts[parts.Length - 1]] = p.Property.Value;
            }

            // ---------------------------------------------------
            // now convert the map to a json string.
            // ---------------------------------------------------
            using var output = new MemoryStream();
            using var writer = new Utf8JsonWriter(output, new JsonWriterOptions {Indented = true});
            writer.WriteStartObject();
            WriteObject(writer, map);
            writer.WriteEndObject();
            writer.Flush();
            output.Position = 0;
            using var streamReader = new StreamReader(output);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        ///     Convert a dictionary (which represents an object)
        ///     to json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="map"></param>
        private static void WriteObject(Utf8JsonWriter writer, ConcurrentDictionary<string, object> map)
        {
            foreach (var kv in map)
                switch (kv.Value)
                {
                    case ConcurrentDictionary<string, object> obj:
                        writer.WriteStartObject(kv.Key);
                        WriteObject(writer, obj);
                        writer.WriteEndObject();
                        continue;
                    default:
                        WriteProperty(writer, kv.Key, kv.Value);
                        break;
                }
        }

        /// <summary>
        ///     Writes a property to the json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void WriteProperty(Utf8JsonWriter writer, string name, object value)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull(name);
                    return;
                case string stringValue:
                    writer.WriteString(name, stringValue);
                    break;
                default:
                    throw new InvalidOperationException("Unhandled data type: " + value.GetType().FullName);
            }
        }
    }
}