using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record Property(string Name, List<Property> ChildProperties);

    public class DocMerger3
    {
        private readonly JsonDocument structureModel;
        private readonly List<DocumentToMerge> toMerge;

        private DocMerger3(JsonDocument structureModel, IEnumerable<DocumentToMerge> toMerge)
        {
            this.toMerge = toMerge.ToList();
            this.structureModel = structureModel;
        }

        /// <summary>
        ///     Merge multiple documents into one.
        /// </summary>
        /// <param name="structureModel"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        public static async Task<ObjectValue> Merge(JsonDocument structureModel,
            params JsonDocument[] documents)
        {
            return await Task.Run(() =>
            {
                var toMerge = Enumerable.Range(0, documents.Length)
                    .Select(i => new DocumentToMerge(i.ToString(), documents[i]))
                    .ToArray();
                return new DocMerger3(structureModel, toMerge).Merge();
            });
        }

        /// <summary>
        ///     Merge multiple documents into one.
        /// </summary>
        /// <param name="structureModel"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        public static async Task<ObjectValue> Merge(JsonDocument structureModel,
            IEnumerable<DocumentToMerge> documents)
        {
            return await Task.Run(() => new DocMerger3(structureModel, documents.ToList()).Merge());
        }

        /// <summary>
        ///     Entry point - merge.
        ///     This method takes care of the base level of the document.
        ///     This is only used once. Recursion is handled by the parameterized
        ///     merge method.
        /// </summary>
        /// <returns></returns>
        private ObjectValue Merge()
        {
            var objects = structureModel
                .RootElement
                .GetObjects()
                .ToList();

            var mergedObjects = objects
                .Select(o =>
                {
                    var layers = GetLayers(o.Name, toMerge.Select(m => m.Document.RootElement));
                    return Merge("", o, layers);
                })
                .ToList()
                .AsReadOnly();

            var properties = structureModel
                .RootElement
                .GetProperties()
                .Select(p =>
                {
                    var layers = toMerge.Select(m => GetProperty(p.Name, m.Document.RootElement)).ToList();
                    return GetValue(string.Empty, p, layers);
                })
                .ToList()
                .AsReadOnly();

            return new ObjectValue("/", "root", properties, mergedObjects);
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
        private static List<JsonElement> GetLayers(string propertyName, IEnumerable<JsonElement> layerParents)
        {
            return layerParents
                .Select(p => GetProperty(propertyName, p))
                .ToList();
        }

        /// <summary>
        ///     The worker horse.
        ///     Merge JSON elements into each.
        ///     It calls itself to recurse the tree.
        /// </summary>
        /// <param name="path">The path of the current node to be merged.</param>
        /// <param name="model"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        private ObjectValue Merge(string path, JsonProperty model, IEnumerable<JsonElement> layers)
        {
            var newPath = path + "/" + model.Name;

            // merge the objects that are within this object
            var objects = model
                .Value
                .GetObjects()
                .ToList();
            var mergedObjects = objects
                .Select(o =>
                {
                    var nextLayers = GetLayers(o.Name, layers);
                    return Merge(path, o, nextLayers);
                })
                .ToList()
                .AsReadOnly();

            // merge the properties
            var properties = model
                .Value
                .EnumerateObject()
                .Where(o => o.Value.ValueKind == JsonValueKind.String)
                .ToList();
            var mergedProperties = properties
                .Select(p =>
                {
                    var nextLayers = GetLayers(p.Name, layers);
                    return GetValue(newPath, p, nextLayers);
                })
                .ToList()
                .AsReadOnly();

            return new ObjectValue(path + "/" + model.Name, model.Name, mergedProperties, mergedObjects);
        }

        /// <summary>
        ///     Create the property values.
        ///     Determine the transition state
        ///     based on each layer, and the layers that
        ///     preceded it. IE: If the value[x] is undefined, and
        ///     value[x-1] is set, then value[x] inherits value[x-1].
        /// </summary>
        /// <param name="path"></param>
        /// <param name="model"></param>
        /// <param name="parents"></param>
        /// <returns></returns>
        private PropertyValue GetValue(string path, JsonProperty model, IReadOnlyList<JsonElement> parents)
        {
            var layers = new List<PropertyLayer>();

            // set layer 0
            var exists0 = parents[0].ValueKind == JsonValueKind.String;
            layers.Add(new PropertyLayer
            {
                LayerIndex = 0,
                LayerName = toMerge[0].Name,
                Transition = exists0 ? Transition.Set : Transition.DoesntExist,
                Value = exists0 ? parents[0].GetString() : null
            });

            // iterate the remaining layers.
            for (var layerIndex = 1; layerIndex < parents.Count; layerIndex++)
            {
                var previousLayer = layers[layerIndex - 1];
                var l = new PropertyLayer
                {
                    LayerIndex = layerIndex,
                    LayerName = toMerge[layerIndex].Name
                };

                layers.Add(l);
                var existsThisLayer =
                    parents[layerIndex].ValueKind == JsonValueKind.String
                    || parents[layerIndex].ValueKind == JsonValueKind.Null;
                if (existsThisLayer)
                {
                    var lv = parents[layerIndex].GetString();
                    if (lv == null)
                    {
                        l.Transition = Transition.Delete;
                        continue;
                    }

                    // a simple == didn't work here in a previous iteration.
                    // i don't know why... utf8 vs non-utf8?
                    l.Transition = Equals(lv, previousLayer.Value)
                        ? Transition.SetToSameValue
                        : Transition.Set;
                    l.Value = lv;
                    continue;
                }

                if (previousLayer.Transition == Transition.Inherit
                    || previousLayer.Transition.IsSet())
                {
                    l.Transition = Transition.Inherit;
                    l.Value = previousLayer.Value;
                    continue;
                }

                l.Transition = Transition.DoesntExist;
            }

            return new PropertyValue(path + "/" + model.Name, model.Name, layers);
        }
    }
}