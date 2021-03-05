using System;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using Allard.Configinator.Core.Model.Validators;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Validators
{
    public class SchemaTypeValidatorTests
    {
        [Fact]
        public void SelfReference()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoA", "a/a")
                .Build();
            Action test = () => SchemaTypeValidator.Validate(a, new[] {a});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Circular reference. Path=/AtoA, SchemaTypeId=a/a");
        }

        [Fact]
        public void CircularReferenceThrowsException()
        {
            // type a has property of type b
            // type b has property of type c
            // type c has property of type a

            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("BtoC", "a/c")
                .Build();
            var c = SchemaTypeBuilder
                .Create("a/c")
                .AddProperty("CtoA", "a/a")
                .Build();
            Action test = () => SchemaTypeValidator.Validate(a, new[] {b, c});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Circular reference. Path=/AtoB/BtoC/CtoA, SchemaTypeId=a/a");
        }

        [Fact]
        public void NoPropertiesThrowsException()
        {
            // doesn't have properties, so will go boom
            var a = SchemaTypeBuilder
                .Create("a/a")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("blah", SchemaTypeId.String)
                .Build();
            Action test = () => SchemaTypeValidator.Validate(a, new[] {b});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("The SchemaType doesn't have any properties.. Path=/");
        }

        [Fact]
        public void InvalidTypeThrowsException()
        {
            // a refers to b
            // b refers to c
            // but c doesn't exist.
            // sad face.
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", SchemaTypeId.Parse("a/b"))
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("BtoC", SchemaTypeId.Parse("a/c"))
                .Build();
            Action test = () => SchemaTypeValidator.Validate(a, new[] {b});
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Type doesn't exist. Property Path=/AtoB/BtoC. Unknown Type=a/c");

        }
    }
}