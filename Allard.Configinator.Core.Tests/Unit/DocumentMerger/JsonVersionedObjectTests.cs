using System.Text.Json;
using Allard.Configinator.Core.DocumentMerger;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.DocumentMerger
{
    public class JsonVersionedObjectTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public JsonVersionedObjectTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Blah()
        {
            var m = JsonDocument.Parse("{ \"a\": { \"b\": \"\", \"c\": \"\" } }").RootElement;
            var v1 = JsonDocument.Parse("{ \"a\": { \"b\": \"1\", \"c\": \"4\" } }").RootElement;
            var v2 = JsonDocument.Parse("{ \"a\": { \"b\": \"1\" } }").RootElement;
            var v3 = JsonDocument.Parse("{ \"a\": { \"b\": \"3\", \"c\": \"5\" } }").RootElement;

            var versioned = new JsonVersionedObject(m);
            versioned.AddVersion("v1", v1);
            versioned.AddVersion("v2", v2);
            versioned.AddVersion("v3", v3);

            // change b=1 to b=2. a and b change.
            var aModified = JsonDocument.Parse("{ \"a\": { \"b\": \"2\" } }").RootElement;
            versioned.UpdateVersion("v2", aModified);

            var b = versioned.GetObject("a").GetProperty("b");
            b.GetValue("v1").Value.Should().Be("1"); // added
            b.GetValue("v1").Exists.Should().BeTrue();
            b.GetValue("v1").IsSet.Should().BeFalse();
            b.GetValue("v1").Exists.Should().BeTrue();
            b.GetValue("v1").IsChanged.Should().BeFalse();
            b.GetValue("v2").Value.Should().Be("2"); // added, then updated
            b.GetValue("v3").Value.Should().Be("3"); // added

            // c doesn't exist in v2
            var c = versioned.GetObject("a").GetProperty("c");
            c.GetValue("v2").Exists.Should().BeFalse();
        }
    }
}