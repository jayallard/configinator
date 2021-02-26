using System;

namespace Allard.Configinator.Namespaces
{
    public class NamespaceNotFoundException : Exception
    {
        public NamespaceNotFoundException(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}