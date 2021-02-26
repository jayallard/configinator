using System;

namespace Allard.Configinator.Habitats
{
    public class HabitatNotFoundException : Exception
    {
        public HabitatNotFoundException(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}