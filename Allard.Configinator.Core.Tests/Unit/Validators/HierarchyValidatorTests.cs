using System;
using System.Linq;
using Allard.Configinator.Core.Model.Validators;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Validators
{
    public class HierarchyValidatorTests
    {
        [Fact]
        public void DoubleReferenceThrowsException()
        {
            // a references z twice - once directly and once via b. a->z. a->b->z.
            var a = CreateElement("a", "b", "z");
            var b = CreateElement("b", "z");
            var z = CreateElement("z");
            Action test = () => HierarchyValidator.Validate(a, new[] {b, z});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Hierarchy issue. ElementToTest=a, Repeating segment: z");
        }

        private static HierarchyElement CreateElement(string habitatId, params string[] baseHabitatIds)
        {
            return new(habitatId, baseHabitatIds.ToHashSet());
        }

        [Fact]
        public void ReferencesItselfThrowsException()
        {
            // a references z twice - once directly and once via b. a->z. a->b->z.
            var a = CreateElement("a", "b");
            var b = CreateElement("b", "z");
            var z = CreateElement("z", "a");
            Action test = () => HierarchyValidator.Validate(a, new[] {b, z});
            test
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Hierarchy issue. ElementToTest=a, Repeating segment: a");
        }

        [Fact]
        public void CircularReferenceThrowsException()
        {
            var a = CreateElement("a", "b");
            var b = CreateElement("b", "a");
            Action test = () => HierarchyValidator.Validate(a, new[] {b});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Hierarchy issue. ElementToTest=a, Repeating segment: a");
        }

        [Fact]
        public void ElementDoesntExist()
        {
            var a = CreateElement("a", "b");
            var b = CreateElement("b", "z", "y");
            var z = CreateElement("z");

            Action test = () => HierarchyValidator.Validate(a, new[] {b, z});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Element doesn't exist. Element Name=y");
        }

        [Fact]
        public void AllGood()
        {
            var a = CreateElement("a", "b");
            var b = CreateElement("b", "z", "y");
            var z = CreateElement("z");
            var y = CreateElement("y");
            HierarchyValidator.Validate(a, new[] {b, z, y});
        }
    }
}