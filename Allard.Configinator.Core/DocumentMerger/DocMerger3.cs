using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.Infrastructure;

namespace Allard.Configinator.Core.DocumentMerger
{
    public record Property(string name, List<Property> ChildProperties);

    public class DocMerger3
    {
        private readonly JsonDocument structureModel;
        private readonly List<DocumentToMerge> toMerge;

        public static async Task<IEnumerable<MergedProperty>> Merge(JsonDocument structureModel,
            IEnumerable<DocumentToMerge> documents)
        {
            return await Task.Run(() => new DocMerger3(structureModel, documents).Merge());
        }

        private DocMerger3(JsonDocument structureModel, IEnumerable<DocumentToMerge> toMerge)
        {
            this.toMerge = toMerge.ToList();
            this.structureModel = structureModel;
        }

        public List<MergedProperty> Merge()
        {
            // root object contains only objects.
            var result = new List<MergedProperty>();
            var allPerDoc = toMerge
                .Select(m => m.Document.RootElement.EnumerateObject().ToDictionary(p => p.Name))
                .ToList();

            foreach (var property in structureModel.RootElement.EnumerateObject())
            {
                // if (property.Value.ValueKind == JsonValueKind.Object)
                // {
                //     var parents = allPerDoc
                //         .Select(a => a[property.Name])
                //         .ToList();
                //     var props = Merge(property, parents);
                //     result.Add(props);
                // }
                //
                // throw new Exception("Invalid root level structure model");
                var parents = allPerDoc
                    .Select(a => a[property.Name])
                    .ToList();
                var props = Merge(property, parents);
                result.Add(props);
            }

            // var parents = toMerge
            //     .Select(m => m.Document.RootElement.GetProperty())
            // return Merge(structureModel.RootElement, structureModel
            //     .RootElement
            //     .EnumerateObject()
            //     .Where(o => o.Value.ValueKind == JsonValueKind.Object)
            //     .ToList());
            return result;
        }

        private MergedProperty Merge(JsonProperty model, List<JsonProperty> parents)
        {
            var result = new MergedProperty("TODO", new PropertyValue {Name = model.Name}, new List<MergedProperty>());
            if (model.Value.ValueKind == JsonValueKind.String)
            {
                var value = new PropertyValue();
                value.Layers.Add(new PropertyLayer
                {
                    LayerIndex = 0,
                    LayerName = toMerge[0].Name,
                    Transition = parents[0].Value.ValueKind == JsonValueKind.String
                        ? Transition.Set
                        : Transition.DoesntExist,
                    Value = parents[0].Value.ValueKind == JsonValueKind.String ? parents[0].Value.GetString() : null
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
                    //var layerFlattened = flattenedToMerge[layerIndex];
                    var existsThisLayer = parents[layerIndex].Value.ValueKind == JsonValueKind.String;
                    if (existsThisLayer)
                    {
                        var lv = parents[layerIndex].Value.GetString();
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

                return result;
            }


            if (model.Value.ValueKind == JsonValueKind.Object)
            {
                var allPerDoc = toMerge
                    .Select(m => m.Document.RootElement.EnumerateObject().ToDictionary(p => p.Name))
                    .ToList();
                var p = allPerDoc
                    .Select(a => a[model.Name])
                    .ToList();
                result.Children.Add(Merge(model, p));
                return result;

            }

            throw new Exception("unhandled node type");
        }
    }
}