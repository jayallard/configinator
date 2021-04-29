using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public enum NodeType
    {
        Object,
        String
    }

    [DebuggerDisplay("Name={Name}, Value={Value}")]
    public class Node
    {
        public NodeType NodeType { get; set; } = NodeType.Object;
        public string Name { get; set; }
        public List<Node> Items { get; } = new();

        public IEnumerable<Node> Properties => Items.Where(i => i.IsProperty());
        public IEnumerable<Node> Objects => Items.Where(i => i.IsObject());

        public string Value { get; set; }

        public static Node CreateString(string name, string value = null)
        {
            return new()
            {
                NodeType = NodeType.String,
                Name = name,
                Value = value
            };
        }

        public Node SetName(string name)
        {
            Name = name;
            return this;
        }

        public Node SetValue(string value)
        {
            Value = value;
            return this;
        }

        public Node SetValue(string path, string value)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var current = this;
            for (var i = 0; i < parts.Length - 1; i++) current = current.GetObject(parts[i]);

            current.GetProperty(parts.Last()).SetValue(value);
            return this;
        }

        public Node SetObjectType(NodeType nodeType)
        {
            NodeType = nodeType;
            return this;
        }

        public Node AddString(string name, string value = null)
        {
            Items.Add(CreateString(name, value));
            return this;
        }

        public Node GetObject(string name)
        {
            return Items.Single(o => o.Name == name && o.IsObject());
        }

        public Node Add(Node obj)
        {
            Items.Add(obj);
            return this;
        }

        public bool ObjectExists(string name)
        {
            return Items.Any(o => o.Name == name && o.IsObject());
        }

        public bool PropertyExists(string name)
        {
            return Items.Any(o => o.Name == name && o.IsProperty());
        }

        public Node Add(IEnumerable<Node> objects)
        {
            Items.AddRange(objects);
            return this;
        }

        public Node GetProperty(string name)
        {
            return Items.Single(p => p.Name == name && p.IsProperty());
        }

        public Node Clone()
        {
            return new Node()
                .SetName(Name)
                .SetObjectType(NodeType)
                .SetValue(Value)
                .Add(Items?.Select(o => o.Clone()));
        }
    }
}