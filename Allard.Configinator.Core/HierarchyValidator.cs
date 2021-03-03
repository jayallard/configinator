using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Allard.Configinator.Core
{
    public record HierarchyElement(string Name, IReadOnlySet<string> Bases);

    public class HierarchyValidator
    {
        private readonly HierarchyElement toTest;
        private readonly Dictionary<string, HierarchyElement> otherElements;
        
        private HierarchyValidator(HierarchyElement elementToTest, IEnumerable<HierarchyElement> otherElements)
        {
            toTest = elementToTest.EnsureValue(nameof(elementToTest));
            this.otherElements = otherElements
                .EnsureValue(nameof(otherElements))
                .ToDictionary(h => h.Name);
        }


        private void Validate()
        {
            var encountered = new HashSet<string> {toTest.Name};
            foreach (var b in toTest.Bases)
            {
                Validate(b, encountered);
            }
        }

        private void Validate(string baseName, ISet<string> encountered)
        {
            if (encountered.Contains(baseName))
            {
                throw new InvalidOperationException("Hierarchy issue. ElementToTest=" + toTest.Name + ", Repeating segment: " + baseName);
            }

            encountered.Add(baseName);
            if (!otherElements.TryGetValue(baseName, out var nextElement))
            {
                throw new InvalidOperationException("Element doesn't exist; " + baseName);
            }
            
            foreach (var b in nextElement.Bases)
            {
                Validate(b, encountered);
            }
        }

        public static void Validate(HierarchyElement toTest, IEnumerable<HierarchyElement> otherElements)
        {
            new HierarchyValidator(toTest, otherElements).Validate();
        }
    }
}