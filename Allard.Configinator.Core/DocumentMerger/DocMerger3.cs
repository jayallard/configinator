using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record Property(string name, List<Property> ChildProperties);

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

        public static async Task<IEnumerable<MergedProperty>> Merge(JsonDocument structureModel,
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

        public static async Task<IEnumerable<MergedProperty>> Merge(JsonDocument structureModel,
            params DocumentToMerge[] documents)
        {
            return await Task.Run(() => new DocMerger3(structureModel, documents).Merge());
        }

        public static async Task<IEnumerable<MergedProperty>> Merge(JsonDocument structureModel,
            IEnumerable<DocumentToMerge> documents)
        {
            return await Task.Run(() => new DocMerger3(structureModel, documents.ToList()).Merge());
        }

        public List<MergedProperty> Merge()
        {
            var results = new List<MergedProperty>();
            foreach (var p in structureModel.RootElement.EnumerateObject())
            {
                var parents =
                    toMerge.Select(d => d.Document.RootElement.TryGetProperty(p.Name, out var e) ? e : default)
                        .ToList();
                results.Add(Merge("", p, parents));
            }

            return results;
        }

        private MergedProperty Merge(string path, JsonProperty model, List<JsonElement> parents)
        {
            var value = new PropertyValue {Name = model.Name};
            if (model.Value.ValueKind == JsonValueKind.String) return GetValue(path, model, parents);

            if (model.Value.ValueKind != JsonValueKind.Object) throw new Exception("Invalid model");

            var newPath = path + "/" + model.Name;
            var result = new MergedProperty(newPath, value, new List<MergedProperty>());
            foreach (var p in model.Value.EnumerateObject())
            {
                var next =
                    parents
                        .Select(d =>
                        {
                            if (d.ValueKind == JsonValueKind.Undefined) return d;

                            return d.TryGetProperty(p.Name, out var e) ? e : default;
                        }).ToList();
                result.Children.Add(Merge(newPath, p, next));
            }

            return result;
        }

        private MergedProperty GetValue(string path, JsonProperty model, List<JsonElement> parents)
        {
            var value = new PropertyValue {Name = model.Name};
            var prop = new MergedProperty(path + "/" + model.Name, value, new List<MergedProperty>());

            // set layer 0
            var exists0 = parents[0].ValueKind == JsonValueKind.String;
            value.Layers.Add(new PropertyLayer
            {
                LayerIndex = 0,
                LayerName = toMerge[0].Name,
                Transition = exists0 ? Transition.Set : Transition.DoesntExist,
                Value = exists0 ? parents[0].GetString() : null
            });

            for (var layerIndex = 1; layerIndex < parents.Count; layerIndex++)
            {
                var previousLayer = value.Layers[layerIndex - 1];
                var l = new PropertyLayer
                {
                    LayerIndex = layerIndex,
                    LayerName = toMerge[layerIndex].Name
                };

                value.Layers.Add(l);
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

            return prop;
        }
    }
}