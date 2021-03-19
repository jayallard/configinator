using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.SchemaValidatorTests
{
    public class DocValidatorTests
    {
        [Fact]
        public void FailsIfRequiredObjectIsMissing()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("test", SchemaTypeId.String)
                .Build();

            // the property "AtoB" is required but missing.
            var doc = JsonDocument.Parse("{}").RootElement;
            var results = new DocValidator(new[] {a, b})
                .Validate(SchemaTypeId.Parse("a/a"), doc)
                .ToList();
            results.Single().Code.Should().Be("RequiredObjectMissing");
        }

        [Fact]
        public void PassesIfOptionalObjectIsMissing()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b", false, true)
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("test", SchemaTypeId.String)
                .Build();

            // the property "AtoB" is required but missing.
            var doc = JsonDocument.Parse("{}").RootElement;
            new DocValidator(new[] {a, b})
                .Validate(SchemaTypeId.Parse("a/a"), doc)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void FailsIfRequiredPropertyIsMissing()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("test", SchemaTypeId.String)
                .Build();

            // the property "AtoB" is required but missing.
            var doc = JsonDocument.Parse("{ \"AtoB\": { } }").RootElement;
            var results = new DocValidator(new[] {a, b})
                .Validate(SchemaTypeId.Parse("a/a"), doc)
                .ToList();
            results.Single().Code.Should().Be("RequiredPropertyMissing");
        }

        [Fact]
        public void PassesIfOptionalPropertyIsMissing()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("test", SchemaTypeId.String, false, true)
                .Build();

            // the property "AtoB" is required but missing.
            var doc = JsonDocument.Parse("{ \"AtoB\": { } }").RootElement;
            new DocValidator(new[] {a, b})
                .Validate(SchemaTypeId.Parse("a/a"), doc)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void FailsIfRequiredPropertyValueIsMissing()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("test", SchemaTypeId.String)
                .Build();

            // the property "AtoB" is required but missing.
            var doc = JsonDocument.Parse("{ \"AtoB\": { \"test\": null } }").RootElement;
            var results = new DocValidator(new[] {a, b})
                .Validate(SchemaTypeId.Parse("a/a"), doc)
                .ToList();
            results.Single().Code.Should().Be("RequiredPropertyValueMissing");
        }

        [Fact]
        public void PassesIfOptionalPropertyValueIsMissingValue()
        {
            var a = SchemaTypeBuilder
                .Create("a/a")
                .AddProperty("AtoB", "a/b")
                .Build();
            var b = SchemaTypeBuilder
                .Create("a/b")
                .AddProperty("test", SchemaTypeId.String, false, true)
                .Build();

            // the property "AtoB" is required but missing.
            var doc = JsonDocument.Parse("{ \"AtoB\": { \"test\": null } }").RootElement;
            new DocValidator(new[] {a, b})
                .Validate(SchemaTypeId.Parse("a/a"), doc)
                .Should()
                .BeEmpty();
        }
    }
}