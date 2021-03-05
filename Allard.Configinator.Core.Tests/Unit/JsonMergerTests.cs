using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.DocumentValidator;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class JsonMergerTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public JsonMergerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Merge()
        {
            var top = JsonDocument.Parse("{ \"test\": \"hi\", \"obj\": { \"hello\": \"world\" } }").RootElement;
            var middle = JsonDocument.Parse("{ \"santa\": \"claus\", \"x\": \"y\" }").RootElement;
            var bottom = JsonDocument
                .Parse("{ \"santa\": \"claus\", \"x\": null, \"obj\": { \"hello\": \"moon\", \"a\": \"b\", \"n1\": { \"n2\": { \"n3\": { \"totally\": \"deep\" } } }  } }")
                .RootElement;

            var merge = new List<DocName>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("middle", 1, new JsonObjectNode("", middle)),
                new("bottom", 2, new JsonObjectNode("", bottom))
            };

            var node = new JsonObjectNode(string.Empty, top);
            var result = new ObjectMerger(merge).Merge();
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
        }

        [Fact]
        public void DeleteProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": null }").RootElement;

            var merge = new List<DocName>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var node = new JsonObjectNode(string.Empty, top);
            var result = new ObjectMerger(merge).Merge();
            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().BeNull();
            prop.History[0].ActionType.Should().Be(ActionType.Set);
            prop.History[1].ActionType.Should().Be(ActionType.Deleted);
            
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
        }
        
        /// <summary>
        /// Doc 0 doesn't have the property.
        /// Doc 1 does.
        /// Back fill the history for doc 0 with DoesntExist.
        /// </summary>
        [Fact]
        public void PropertySetInSecondDocument()
        {
            var top = JsonDocument.Parse("{  }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;

            var merge = new List<DocName>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var result = new ObjectMerger(merge).Merge();
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.History[0].ActionType.Should().Be(ActionType.DoesntExist);
            prop.History[1].ActionType.Should().Be(ActionType.Set);
            
        }
        
        
        [Fact]
        public void InheritProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            // will inherit test=world
            var bottom = JsonDocument.Parse("{  }").RootElement;

            var merge = new List<DocName>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 2, new JsonObjectNode("", bottom))
            };

            var result = new ObjectMerger(merge).Merge();
            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.History[0].ActionType.Should().Be(ActionType.Set);
            prop.History[1].ActionType.Should().Be(ActionType.Inherited);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
        }
        
        public class ObjectMerger
        {
            private readonly Dictionary<string, PropertyValue> merged = new();

            private readonly List<DocName> toMerge;

            public ObjectMerger(IEnumerable<DocName> toMerge)
            {
                this.toMerge = toMerge.EnsureValue(nameof(toMerge)).ToList();
            }

            public Dictionary<string, PropertyValue>  Merge()
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
                var incomplete = merged.Where(m => m.Value.History.Count < toMerge.Count);
                foreach (var i in incomplete)
                {
                    var prop = i.Value;
                    for (var h = 0; h < toMerge.Count; h++)
                    {
                        var historyNode = prop.History.FirstOrDefault(x => x?.DocName?.Order == h);
                        if (historyNode != null)
                        {
                            continue;
                        }

                        // the first document didn't have the property.
                        if (h == 0)
                        {
                            prop.History.Insert(0, new PropertyHistoryItem
                            {
                                ActionType = ActionType.DoesntExist,
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
                            ActionType = previous.ActionType switch
                            {
                                ActionType.Deleted => ActionType.DoesntExist,
                                ActionType.Inherited => ActionType.Inherited,
                                ActionType.Set => ActionType.Inherited,
                                ActionType.DoesntExist => ActionType.DoesntExist,
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
            
            
            private void Merge(DocName doc, IObjectNode node, string path)
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
                        ActionType = property.Value == null ? ActionType.Deleted : ActionType.Set,
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

        public class PropertyHistoryItem
        {
            public DocName DocName { get; set; }
            public IObjectNode Object { get; set; }
            public IPropertyNode Property { get; set; }

            public ActionType ActionType { get; set; }
            public string ActionTypeString => ActionType.ToString();
            public string ReferencedDoc { get; set; }
            public object Value { get; set; }
        }


        public record DocName(string Name, int Order, IObjectNode Doc);

        public class PropertyMergeNode
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }


        public enum ActionType
        {
            Set,
            Deleted,
            DoesntExist,
            Inherited
        }

        /*
         
         {
            "test": "hi",
            "santa": "claus",
            "x": "y",
            "obj": {
                "hello": "world"
                "a": "b"
            }
         }
         
         {
            "test": "hi",
            "$$test": {
                "value": "hi",
                "actions": [
                    {
                        "doc": "name of doc"
                        "action": "set-by,inherited-from,deleted-by",
                    }
                ]
            }             
         }
         */


        /*
         * test = hi
         * obj.hello=world
         *
         * santa=claus
         * x=y
         *
         * santa=claus
         * x=null
         * obj.hello=moon
         * obj.a=b
         */
    }
}