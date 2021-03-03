using System;
using System.Collections.Generic;
using FluentAssertions;
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
            Action test = () => HierarchyValidator.Validate(a, new[] {b, z});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Hierarchy issue. ElementToTest=a, Repeating segment: z");
        }

        [Fact]
        public void ReferencesItselfThrowsException()
        {
            // a references z twice - once directly and once via b. a->z. a->b->z.
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"z"});
            var z = new HierarchyElement("z", new HashSet<string>{"a"});
            Action test = () => HierarchyValidator.Validate(a, new[] {b, z});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Hierarchy issue. ElementToTest=a, Repeating segment: a");
        }

        [Fact]
        public void CircularReferenceThrowsException()
        {
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"a"});
            Action test = () => HierarchyValidator.Validate(a, new[] {b});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Hierarchy issue. ElementToTest=a, Repeating segment: a");
        }

        [Fact]
        public void ElementDoesntExist()
        {
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"z", "y"});
            var z = new HierarchyElement("z", new HashSet<string>());
            Action test = () => HierarchyValidator.Validate(a, new[] {b, z});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Element doesn't exist. Element Name=y");
        }
        
        [Fact]
        public void AllGood()
        {
            var a = new HierarchyElement("a", new HashSet<string> {"b"});
            var b = new HierarchyElement("b", new HashSet<string> {"z", "y"});
            var z = new HierarchyElement("z", new HashSet<string>());
            var y = new HierarchyElement("y", new HashSet<string>());
            HierarchyValidator.Validate(a, new[] {b, z, y});
        }
    }
}