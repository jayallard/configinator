using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.DocumentMerger
{
    /// <summary>
    ///     Merge multiple documents into one.
    ///     This crawls each document, and contributes its contents
    ///     to a result document.
    ///     If a value is set to null, it is deleted from the result set.
    ///     A subsequent document may add it back.
    ///     When a value exists in multiple documents, last one wins.
    /// </summary>
    public class DocMerger
    {
        // the result doc.
        private readonly Dictionary<string, PropertyValue> merged = new();

        private readonly List<OrderedDocumentToMerge> toMerge;

        private DocMerger(IEnumerable<DocumentToMerge> toMerge)
        {
            var index = 0;
            this.toMerge = toMerge
                .EnsureValue(nameof(toMerge))
                .Select(d => new OrderedDocumentToMerge(d, index++))
                .ToList();
        }

        public static Task<IEnumerable<MergedProperty>> Merge(IEnumerable<DocumentToMerge> documents)
        {
            return Task.Run(() => new DocMerger(documents).Merge());
        }

        private IEnumerable<MergedProperty> Merge()
        {
            foreach (var m in toMerge) Merge(m, m.Doc.Document, "");

            FillInTheBlanks();
            return merged
                .Select(kv => new MergedProperty(kv.Key, kv.Value));
        }

        private void FillInTheBlanks()
        {
            // iterate all of the properties in the result doc.
            foreach (var (_, currentObject) in merged)
                // if there are 5 merge docs, then every
                // property will have 5 layer items.
                // iterate the layer items. 
                // some will be missing. (IE: if something was added in doc 2,
                // then the layer is missing in doc1. if something is deleted
                // in doc 3, then it is missing in doc 4 (unless doc 4 puts it back).
                for (var h = 0; h < toMerge.Count; h++)
                {
                    // get the layer node for the current index.
                    var currentLayerNode = currentObject
                        .Layers
                        .FirstOrDefault(x => x?.DocName?.Order == h);

                    // if the layer node exists, then see if it needs any adjustments.
                    // IE: if item #2 is SET, and item #3 is SET, then change #3 to
                    // SetToSameValue.
                    if (currentLayerNode != null)
                    {
                        // if we're at index 0, then there's nothing to do.
                        if (h == 0) continue;

                        // if the previous version is set, and the current
                        // version is set, then change current to SetToSame.
                        // both did explicit sets, but to the same value.
                        if (currentLayerNode.Transition.IsSet() &&
                            currentObject.Layers[h - 1].Transition.IsSet())
                            currentLayerNode.Transition = Transition.SetToSameValue;

                        continue;
                    }

                    // the first document didn't have the property.
                    // this means it was added by a future document.
                    // insert layer stating that it didn't exist.
                    if (h == 0)
                    {
                        currentObject.Layers.Insert(0, new PropertyLayer
                        {
                            Transition = Transition.DoesntExist,
                            DocName = null,
                            Object = null,
                            Property = null,
                            ReferencedDoc = null,
                            Value = null
                        });
                        continue;
                    }

                    // a layer item > 0 that doesn't exist.
                    // back fill the layer by looking at the previous layer item.
                    var previous = currentObject.Layers[h - 1];
                    currentObject.Layers.Insert(h, new PropertyLayer
                    {
                        Transition = previous.Transition switch
                        {
                            // if the previous item deleted it,
                            // then it no longer exists.
                            Transition.Delete => Transition.DoesntExist,

                            // if the previous was inherited, then its still inherited.
                            Transition.Inherit => Transition.Inherit,

                            // if the previous assigned a value,
                            // this this inherits that value.
                            Transition.SetToSameValue => Transition.Inherit,
                            Transition.Set => Transition.Inherit,

                            // if it didn't exist in the previous,
                            // the it still doesn't exist.
                            Transition.DoesntExist => Transition.DoesntExist,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        DocName = null,
                        Object = null,
                        Property = null,
                        ReferencedDoc = null,
                        Value = null
                    });
                }
        }

        /// <summary>
        ///     Merge a document into the result object.
        ///     Recursive.
        /// </summary>
        /// <param name="doc">This is the doc that the node belongs to. The doc contains many nodes.</param>
        /// <param name="node">The node to merge into the result set.</param>
        /// <param name="path"></param>
        private void Merge(OrderedDocumentToMerge doc, IObjectNode node, string path)
        {
            // TODO: currently, doc isn't needed.
            // keep it around until complete, then delete if still not needed.

            // iterate the properties and add them to the result.
            foreach (var property in node.GetPropertyValues())
            {
                var propertyPath = path + "/" + property.Name;

                // add the property to the results, if it doesn't already exist.
                if (!merged.ContainsKey(propertyPath))
                    merged.Add(propertyPath, new PropertyValue {Name = property.Name});

                // get the property, and set its new value.
                var resultProperty = merged[propertyPath];
                resultProperty.Value = property.Value;
                resultProperty.Layers.Add(new PropertyLayer
                {
                    // if the value is null, then delete.
                    // (if the property was missing, instead of null, then it would carry forward)
                    Transition = property.Value == null
                        ? Transition.Delete
                        : Transition.Set,
                    DocName = doc,
                    Property = property,
                    ReferencedDoc = null,
                    Object = node,
                    Value = property.Value
                });
            }

            // iterate the objects in the node, and merge them too.
            foreach (var n in node.GetObjectNodes())
            {
                var nodePath = path + "/" + n.Name;
                Merge(doc, n, nodePath);
            }
        }
    }
}