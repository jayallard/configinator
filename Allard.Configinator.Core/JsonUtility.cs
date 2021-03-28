using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Allard.Configinator.Core
{
    public static class JsonUtility
    {
        /// <summary>
        /// Creates a json document that stubs out the specified path.
        /// The path is / delimited. IE: /a/b/c.
        /// It will create a doc:
        ///  { "a": { "b": { "c": VALUE } } }
        /// Value may be a string or an object. The value is inserted
        /// as-is.
        /// WARNING: this isn't yet hardened. Invalid input will break it.
        /// </summary>
        /// <param name="path">Create a Json document of this structure. / delimited.</param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static JsonDocument Expand(string path, JsonDocument value)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return value;
            }

            var parts = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
            var json = new StringBuilder();

            // open object
            if (parts.Length > 1)
            {
                json.Append("{ ");
            }

            // open structure
            for (var i = 0; i < parts.Length - 1; i++)
            {
                json
                    .Append("\"")
                    .Append(parts[i])
                    .Append("\": { ");
            }

            // insert the json as it is in the request. we know it's valid.
            json
                .Append("\"")
                .Append(parts.Last())
                .Append("\": ");
            
            if (value.RootElement.ValueKind == JsonValueKind.String)
            {
                // todo: escape
                json.Append("\"")
                    .Append(value.RootElement.GetString())
                    .Append("\"");
            }
            else if (value.RootElement.ValueKind == JsonValueKind.Object)
            {
                json.Append(value.RootElement.ToString());
            }
            else
            {
                throw new NotImplementedException("Unsupported json node type: " + value.RootElement.ValueKind);
            }

            // close structure
            for (var i = 0; i < parts.Length - 1; i++)
            {
                json.Append(" }");
            }

            // close object
            if (parts.Length > 1)
            {
                json.Append(" }");
            }

            return JsonDocument.Parse(json.ToString());
        }

    }
}