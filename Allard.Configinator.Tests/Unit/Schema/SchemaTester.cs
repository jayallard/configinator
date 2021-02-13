using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using FluentAssertions;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit.Schema
{
    public class SchemaTester
    {
        private readonly ITestOutputHelper testOutputHelper;
        private const int indentLevel = 3;
        private readonly string oneIndent = new(' ', indentLevel);

        public SchemaTester(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public async Task Test(string schemaId)
        {
            var schema = await TestUtility.CreateSchemaParser().GetSchema(schemaId);
            var expected = await TestUtility.GetExpectedResolution(schemaId);

            schema.Id.Should().Be(expected.StringValue("id"));

            testOutputHelper.WriteLine("Schema: " + schema.Id);
            var expectedPathsNode = (YamlMappingNode) expected["paths"];
            schema.Paths.Count.Should().Be(expectedPathsNode.Children.Count);
            foreach (var expectedPathNode in expectedPathsNode)
            {
                var expectedPath = (string) expectedPathNode.Key;
                testOutputHelper.WriteLine(oneIndent + "Path: " + expectedPath);
                var actualPath = schema.Paths.Single(s => s.Path == expectedPath);

                var expectedPathPropertiesNode = (YamlMappingNode) expectedPathNode.Value["properties"];
                actualPath.Properties.Count.Should().Be(expectedPathPropertiesNode.Children.Count);
                VerifyProperties(expectedPathPropertiesNode, actualPath.Properties, 2, "");
            }
        }

        private void VerifyProperties(YamlMappingNode expected, List<Property> actual, int level, string propertyName)
        {
            var space = new string(' ', level * indentLevel);
            foreach (var expectedPropertyNode in expected)
            {
                var expectedPropertyName = (string) expectedPropertyNode.Key;
                var expectedPropertyPath = propertyName + "/" + expectedPropertyName;
                var expectedType = expectedPropertyNode.Value.StringValue("type");

                testOutputHelper.WriteLine(space + expectedPropertyName + " [" + expectedType + "]");
                var actualProperty = actual.SingleOrDefault(p => p.Name == expectedPropertyName);
                actualProperty.Should().NotBeNull("actual property not found: " + expectedPropertyPath);
                Debug.Assert(actualProperty != null);
                actualProperty.TypeId.FullId.Should().Be(expectedType);
                var map = (YamlMappingNode) expectedPropertyNode.Value;
                if (!map.Children.ContainsKey("properties"))
                {
                    continue;
                }

                var group = (PropertyGroup) actualProperty;
                VerifyProperties((YamlMappingNode) map["properties"], group.Properties.ToList(), level + 1,
                    expectedPropertyPath);
            }
        }
    }
}