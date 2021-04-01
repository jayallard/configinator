using System;

namespace Allard.Configinator.Core
{
    public static class ExtensionMethods
    {
        public static void Visit<TId, TValue>(this Tree<TId, TValue>.Leaf<TId, TValue> leaf,
            Action<Tree<TId, TValue>.Leaf<TId, TValue>> visit) where TId : class
        {
            visit(leaf);
            foreach (var child in leaf.Children)
            {
                child.Visit(visit);
            }
        }
    }
}