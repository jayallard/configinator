using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class DocMerger4
    {
        private readonly List<DocumentToMerge> documents;
        private readonly JsonDocument model;
        private readonly List<string> layerNames;

        public DocMerger4(List<DocumentToMerge> documents, JsonDocument model)
        {
            this.documents = documents;
            this.model = model;
            layerNames = documents.Select(d => d.Name).ToList();
        }
        
        /// <summary>
        ///     Gets a  form the parent element.
        ///     If the property doesn't exist,
        ///     returns an Undefined json element.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="parentElement"></param>
        /// <returns></returns>
        private static JsonElement GetProperty(string propertyName, JsonElement parentElement)
        {
            if (parentElement.ValueKind == JsonValueKind.Undefined) return parentElement;
            return parentElement.TryGetProperty(propertyName, out var existing) ? existing : default;
        }

        /// <summary>
        ///     Gets the property from each of the elements.
        ///     If the property doesn't exist, it returns
        ///     Undefined for that item.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="layerParents"></param>
        /// <returns></returns>
        private static List<JsonElement> GetProperties(string propertyName, IEnumerable<JsonElement> layerParents)
        {
            return layerParents
                .Select(p => GetProperty(propertyName, p))
                .ToList();
        }

        public ObjectValue2 Merge()
        {
            var objectNode = new ObjectValue2(string.Empty, string.Empty, layerNames);
            
            var jsonObjects = model
                .RootElement
                .EnumerateObject()
                .Where(o => o.Value.ValueKind == JsonValueKind.Object);
            foreach (var o in jsonObjects)
            {
                
            }
            var stringProperties = model
                .RootElement
                .EnumerateObject()
                .Where(o => o.Value.ValueKind == JsonValueKind.String);
        }

        private ObjectValue2 Get(JsonElement modelNode, List<JsonElement> layerNodes)
        {
            
        }
    }
}