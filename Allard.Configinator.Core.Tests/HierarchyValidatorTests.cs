using System.Collections.Generic;
using Xunit;

namespace Allard.Configinator.Core.Tests
{
    public class HierarchyValidatorTests
    {
        [Fact]
        public void DoubleReferenceThrowsException()
        {
            // a references z twice - once directly and once via b. a->z. a->b->z.
            var a = new HierarchyElement("a", new HashSet<string> {"b", "z"});
            var b = new HierarchyElement("b", new HashSet<string> {"z"});
            var z = new HierarchyElement("z", new HashSet<string>());
            HierarchyValidator.Validate(a, new[] {b, z});
        }

        [Fact]
        public void ReferencesItselfThrowsException()
        {
            // a references z twice - once directly and once via b. a->z. a->b->z.
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"z"});
            var z = new HierarchyElement("z", new HashSet<string>{"a"});
            HierarchyValidator.Validate(a, new[] {b, z});
        }

        [Fact]
        public void CircularReferenceThrowsException()
        {
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"a"});
            HierarchyValidator.Validate(a, new[] {b});
        }

        [Fact]
        public void ElementDoesntExist()
        {
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"z", "y"});
            var z = new HierarchyElement("z", new HashSet<string>());
            HierarchyValidator.Validate(a, new[] {b, z});
        }
        
        [Fact]
        public void AllGood()
        {
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"z", "y"});
            var z = new HierarchyElement("z", new HashSet<string>());
            var y = new HierarchyElement("y", new HashSet<string>());
            HierarchyValidator.Validate(a, new[] {b,z,y});
        }

    }
}