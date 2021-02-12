using System;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit.Schema
{
    public class SchemaParserTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SchemaParserTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task DoEverythingBetter()
        {
            var schema = await TestUtility.CreateSchemaParser().GetSchema("exhaustive");
            var expected = await TestUtility.GetExpectedResolution("exhaustive");

            schema.Id.Should().Be(expected.StringValue("id"));

            var expectedPathsNode = (YamlMappingNode) expected["paths"];
            schema.Paths.Count.Should().Be(expectedPathsNode.Children.Count);
            foreach (var expectedPathNode in expectedPathsNode)
            {
                var expectedPath = (string) expectedPathNode.Key;
                var actualPath = schema.Paths.Single(s => s.Path == expectedPath);

                var expectedPathPropertiesNode = (YamlMappingNode) expectedPathNode.Value["properties"];
                actualPath.Properties.Count.Should().Be(expectedPathPropertiesNode.Children.Count);
                
                foreach (var expectedPropertyNode in expectedPathPropertiesNode)
                {
                    var expectedPropertyName = (string) expectedPropertyNode.Key;
                    var actualProperty = actualPath.Properties.Single(p => p.Name == expectedPropertyName);
                    actualProperty.TypeId.FullId.Should().Be(expectedPropertyNode.Value.StringValue("type"));
                }
            }
        }
    }
}