using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.DocumentValidator;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.SchemaValidatorTests
{
    public class JsonObjectNodeTests
    {
        [Fact]
        public void Parse()
        {
            var doc = JsonDocument.Parse(
                "{ \"name\": { \"first\": \"santa\", \"last\": \"claus\", \"nested\": { \"n1\": \"n2\" } }," +
                " \"alrighty\": \"then\", \"obj\": { \"a\": \"b\" }, \"easter\": \"bunny\"  }");
            var node = new JsonObjectNode(string.Empty, doc.RootElement);

            var objs = node.GetObjectNodes().ToDictionary(o => o.Name);
            objs.Count.Should().Be(2);

            var props = node.GetPropertyValues().ToDictionary(p => p.Name);
            props.Count.Should().Be(2);
            props["alrighty"].Value.Should().Be("then");
            props["easter"].Value.Should().Be("bunny");

            objs["name"].GetObjectNodes().Count().Should().Be(1);
            objs["name"].GetObjectNodes().Single().Name.Should().Be("nested");
            objs["name"].GetObjectNodes().Single().GetPropertyValues().Single().Name.Should().Be("n1");
            objs["name"].GetObjectNodes().Single().GetPropertyValues().Single().Value.Should().Be("n2");


            var nameProps = objs["name"].GetPropertyValues().ToDictionary(o => o.Name);
            nameProps.Count.Should().Be(2);
            nameProps["first"].Value.Should().Be("santa");
            nameProps["last"].Value.Should().Be("claus");
        }
    }
}