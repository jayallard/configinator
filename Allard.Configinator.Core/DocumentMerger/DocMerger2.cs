using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class DocMerger2
    {
        private readonly JsonDocument structureModel;
        private readonly List<DocumentToMerge> toMerge;

        private DocMerger2(JsonDocument structureModel, IEnumerable<DocumentToMerge> toMerge)
        {
            var index = 0;
            this.toMerge = toMerge.ToList();
            this.structureModel = structureModel;
        }

        public static async Task<IEnumerable<MergedProperty>> Merge(JsonDocument structureModel,
            IEnumerable<DocumentToMerge> documents)
        {
            return await Task.Run(() => new DocMerger2(structureModel, documents).Merge());
        }

        private IEnumerable<MergedProperty> Merge()
        {
            var flattenedModel = Flatten(structureModel, true).Select(m => m.Key).ToHashSet();
            var flattenedToMerge = toMerge
                .Select(m => Flatten(m.Document, true))
                .ToList();

            var results = new List<MergedProperty>();
            foreach (var propertyPath in flattenedModel)
            {
                var propertyName = propertyPath.Split("/").Last();
                var value = new PropertyValue {Name = propertyName};
                var prop = new MergedProperty(propertyPath, value);
                results.Add(prop);

                // set layer 0
                var v = flattenedToMerge[0];
                var exists = v.ContainsKey(propertyPath);
                value.Layers.Add(new PropertyLayer
                {
                    LayerIndex = 0,
                    LayerName = toMerge[0].Name,
                    Transition = exists ? Transition.Set : Transition.DoesntExist,
                    Value = exists ? v[propertyPath] : null
                });

                for (var layerIndex = 1; layerIndex < flattenedToMerge.Count; layerIndex++)
                {
                    var previousLayer = value.Layers[layerIndex - 1];
                    var l = new PropertyLayer
                    {
                        LayerIndex = layerIndex,
                        LayerName = toMerge[layerIndex].Name
                    };

                    value.Layers.Add(l);
                    var layerFlattened = flattenedToMerge[layerIndex];
                    var existsThisLayer = layerFlattened.ContainsKey(propertyPath);
                    if (existsThisLayer)
                    {
                        var lv = layerFlattened[propertyPath];
                        if (lv == null)
                        {
                            l.Transition = Transition.Delete;
                            continue;
                        }

                        l.Transition = Object.Equals(lv, previousLayer.Value)
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
            }

            return results;
        }

        public static Dictionary<string, object> Flatten(JsonDocument document, bool keepNulls)
        {
            var values = new Dictionary<string, object>();
            Flatten(document.RootElement, string.Empty, values, keepNulls);
            return values;
        }

        private static void Flatten(JsonElement element, string path, Dictionary<string, object> values, bool keepNulls)
        {
            foreach (var e in element.EnumerateObject())
            {
                var p = path + "/" + e.Name;
                if (e.Value.ValueKind == JsonValueKind.String || (keepNulls && e.Value.ValueKind == JsonValueKind.Null))
                {
                    values[p] = e.Value.GetString();
                    continue;
                }

                if (e.Value.ValueKind == JsonValueKind.Object)
                {
                    Flatten(e.Value, p, values, keepNulls);
                }
            }
        }
    }
}