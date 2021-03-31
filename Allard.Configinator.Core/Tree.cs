using System;
using System.Collections.Generic;

namespace Allard.Configinator.Core
{
    public class Tree<TId, TValue> where TId : class
    {
        private readonly Dictionary<TId, Leaf<TId, TValue>> all = new();

        public Leaf<TId, TValue> Root { get; }
        
        public Tree(TId rootId, TValue rootValue)
        {
            Root = new Leaf<TId, TValue>(null, rootId, rootValue);
            all[rootId] = Root;
        }

        public Tree<TId, TValue> Add(TId parentId, TId id, TValue value)
        {
            var parent = all[parentId];
            var leaf = new Leaf<TId, TValue>(parent, id, value);
            all.Add(id, leaf);
            parent.AddChild(leaf);
            return this;
        }

        public class Leaf<TLeafId, TLeafValue> where TLeafId: class
        {
            private readonly Dictionary<TLeafId, Leaf<TLeafId, TLeafValue>> children = new();
            public IReadOnlyCollection<Leaf<TLeafId, TLeafValue>> Children => children.Values;
            private Leaf<TLeafId, TLeafValue> Parent { get; }
            public TLeafId Id { get; }
            private TLeafValue Value { get; }
            public Leaf(Leaf<TLeafId, TLeafValue> parent, TLeafId id, TLeafValue value)
            {
                Parent = parent;
                Id = id;
                Value = value;
            }

            public void AddChild(Leaf<TLeafId, TLeafValue> child)
            {
                if (child.Parent != this)
                {
                    throw new InvalidOperationException("Invalid relationship");
                }
                
                children.Add(child.Id, child);
            }
        }
    }
}