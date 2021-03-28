using System.Text.Json;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests
{
    public class JsonUtilityTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public JsonUtilityTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ExpandStringValue()
        {
            var value = "\"testing\"";
            var path = "/a/b/c";
            var doc = JsonDocument.Parse(value);
            var result = JsonUtility.Expand(path, doc);
            result.RootElement.ToString().Should().Be("{ \"a\": { \"b\": { \"c\": \"testing\" } } }");
        }

        [Fact]
        public void SingleString()
        {
            var doc = JsonDocument.Parse("\"testing\"");
            var expand = JsonUtility.Expand(string.Empty, doc);
            expand.RootElement.ToString().Should().Be("testing");
        }

        [Fact]
        public void Object()
        {
            var doc = JsonDocument.Parse("{ \"hello\": { \"bob\": \"the builder\" } }");
            var expand = JsonUtility.Expand("/a/b/c/", doc);
            testOutputHelper.WriteLine(expand.RootElement.ToString());
            expand.RootElement.ToString().Should()
                .Be("{ \"a\": { \"b\": { \"c\": { \"hello\": { \"bob\": \"the builder\" } } } } }");
        }

        [Fact]
        public void NoPath()
        {
            var doc = JsonDocument.Parse("{ \"hello\": { \"bob\": \"the builder\" } }");
            var expand = JsonUtility.Expand(string.Empty, doc);
            expand.Should().Be(doc);
        }
    }
}