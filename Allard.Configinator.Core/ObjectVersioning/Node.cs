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
        public Node(string name, NodeType nodeType)
        {
            Name = name;
            NodeType = nodeType;
        }

        public NodeType NodeType { get; }
        public string Name { get; }
        public List<Node> Items { get; } = new();

        public IEnumerable<Node> Properties => Items.Where(i => i.IsProperty());
        public IEnumerable<Node> Objects => Items.Where(i => i.IsObject());

        public string Value { get; private set; }

        public static Node CreateString(string name, string value = null)
        {
            return new Node(name, NodeType.String).SetValue(value);
        }
        
        public static Node CreateObject(string name = null)
        {
            return new (name ?? "root", NodeType.Object);
        }

        public Node SetValue(string value)
        {
            if (NodeType == NodeType.Object)
            {
                throw new InvalidOperationException("Object Nodes don't have values.");
            }

            Value = value;
            return this;
        }

        public Node SetChildValue(string path, string value)
        {
            FindNode(path).SetValue(value);
            return this;
        }

        public Node FindNode(string path)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var current = this;
            for (var i = 0; i < parts.Length - 1; i++) current = current.GetObject(parts[i]);
            return current.Items.Single(i => i.Name == parts.Last());
        }

        public bool Exists(string path)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var current = this;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ObjectExists(parts[i]))
                {
                    return false;
                }

                current = current.GetObject(parts[i]);
            }

            return ObjectExists(parts.Last()) || PropertyExists(parts.Last());
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
            if (NodeType != NodeType.Object)
            {
                throw new InvalidOperationException("Properties cannot contain properties.");
            }
            Items.AddRange(objects);
            return this;
        }

        public Node GetProperty(string name)
        {
            return Items.Single(p => p.Name == name && p.IsProperty());
        }

        public Node Clone()
        {
            var clone = new Node(Name, NodeType);

            if (this.NodeType == NodeType.Object)
            {
                clone.Add(Items?.Select(o => o.Clone()));
            }
            else
            {
                clone.SetValue(Value);
            }

            return clone;
        }
    }
}