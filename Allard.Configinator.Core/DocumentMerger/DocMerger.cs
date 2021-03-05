using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.DocumentMerger
{
     public class DocMerger
        {
            private readonly Dictionary<string, PropertyValue> merged = new();

            private readonly List<DocumentToMerge> toMerge;

            public DocMerger(IEnumerable<DocumentToMerge> toMerge)
            {
                this.toMerge = toMerge.EnsureValue(nameof(toMerge)).ToList();
            }

            public Dictionary<string, PropertyValue> Merge()
            {
                foreach (var m in toMerge)
                {
                    Merge(m, m.Doc, "");
                }

                FillInTheBlanks();
                return merged;
            }

            private void FillInTheBlanks()
            {
                foreach (var i in merged)
                {
                    var prop = i.Value;
                    for (var h = 0; h < toMerge.Count; h++)
                    {
                        var historyNode = prop.History.FirstOrDefault(x => x?.DocName?.Order == h);
                        if (historyNode != null)
                        {
                            if (h == 0)
                            {
                                continue;
                            }

                            if (historyNode.Transition.AssignedExplicitValue() &&
                                prop.History[h - 1].Transition.AssignedExplicitValue())
                            {
                                historyNode.Transition = Transition.SetToSameValue;
                            }

                            continue;
                        }

                        // the first document didn't have the property.
                        if (h == 0)
                        {
                            prop.History.Insert(0, new PropertyHistoryItem
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

                        // need to figure out the state of the property at this doc.
                        // determine the state by looking at the previous document.
                        var previous = prop.History[h - 1];
                        prop.History.Insert(h, new PropertyHistoryItem
                        {
                            Transition = previous.Transition switch
                            {
                                Transition.Delete => Transition.DoesntExist,
                                Transition.Inherit => Transition.Inherit,
                                Transition.SetToSameValue => Transition.Inherit,
                                Transition.Set => Transition.Inherit,
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
            }


            private void Merge(DocumentToMerge doc, IObjectNode node, string path)
            {
                foreach (var n in node.GetObjectNodes())
                {
                    var nodePath = path + "/" + n.Name;
                    Merge(doc, n, nodePath);
                }

                foreach (var property in node.GetPropertyValues())
                {
                    var propertyPath = path + "/" + property.Name;
                    if (!merged.ContainsKey(propertyPath))
                    {
                        merged.Add(propertyPath, new PropertyValue());
                    }

                    var prop = merged[propertyPath];
                    prop.Value = property.Value;
                    prop.History.Add(new PropertyHistoryItem
                    {
                        Transition = property.Value == null ? Transition.Delete : Transition.Set,
                        DocName = doc,
                        Property = property,
                        ReferencedDoc = null,
                        Object = node,
                        Value = property.Value
                    });
                }
            }

            public class PropertyValue
            {
                public string Name { get; set; }
                public object Value { get; set; }
                public List<PropertyHistoryItem> History { get; } = new();
            }
        }

}