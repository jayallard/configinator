using System;

namespace Allard.Configinator.Realms
{
    public class RealmNotFoundException : Exception
    {
        public RealmNotFoundException(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}