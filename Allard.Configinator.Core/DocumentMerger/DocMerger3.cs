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

        private DocMerger3(JsonDocument structureModel, params DocumentToMerge[] toMerge)
        {
            this.toMerge = toMerge.ToList();
            this.structureModel = structureModel;
        }

        private DocMerger3(JsonDocument structureModel, IEnumerable<DocumentToMerge> toMerge)
        {
            this.toMerge = toMerge.ToList();
            this.structureModel = structureModel;
        }

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

        public static async Task<ObjectValue> Merge(JsonDocument structureModel,
            params DocumentToMerge[] documents)
        {
            return await Task.Run(() => new DocMerger3(structureModel, documents).Merge());
        }

        public static async Task<ObjectValue> Merge(JsonDocument structureModel,
            IEnumerable<DocumentToMerge> documents)
        {
            return await Task.Run(() => new DocMerger3(structureModel, documents.ToList()).Merge());
        }

        private ObjectValue Merge()
        {
            var objects = structureModel
                .RootElement
                .GetObjects()
                .ToList();

            var merged = objects
                .Select(o =>
                {
                    var layers = GetLayers(o.Name, toMerge.Select(m => m.Document.RootElement));
                    return Merge("", o, layers);
                })
                .ToList()
                .AsReadOnly();

            return new ObjectValue("/", "root", new List<PropertyValue>().AsReadOnly(), merged);
        }

        private JsonElement GetLayer(string name, JsonElement layerParent)
        {
            return layerParent.TryGetProperty(name, out var existing) ? existing : default;
        }

        private List<JsonElement> GetLayers(string name, IEnumerable<JsonElement> layerParents)
        {
            return layerParents
                .Select(p => GetLayer(name, p))
                .ToList();
        }

        private ObjectValue Merge(string path, JsonProperty model, List<JsonElement> layers)
        {
            var newPath = path + "/" + model.Name;
            var properties = model
                .Value
                .EnumerateObject()
                .Where(o => o.Value.ValueKind == JsonValueKind.String)
                .ToList();
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

            var mergedProperties = properties
                .Select(p => GetValue(newPath, p, layers))
                .ToList()
                .AsReadOnly();

            return new ObjectValue(path + "/" + model.Name, model.Name, mergedProperties, mergedObjects);
        }

        private PropertyValue GetValue(string path, JsonProperty model, List<JsonElement> parents)
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