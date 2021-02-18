using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit.Schema
{
    public class Tests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Exhaustive()
        {
            await Verify("exhaustive");
        }

        [Fact]
        public async Task InheritAll()
        {
            await Verify("inherit-all");
        }

        private async Task Verify(string id)
        {
            var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas", "TestTypes");
            var typesFolder = Path.Combine(rootPath, "Types");
            var expectedFileName = Path.Combine(rootPath, "ExpectedResolution", id + ".yml");
            var expectedDoc = await TestUtility.GetYamlFromFile(expectedFileName);

            var parser = new SchemaParser(new FileSchemaRepository(typesFolder));
            var expectedTypes = expectedDoc.AsMap("types");
            foreach (var expectedType in expectedTypes)
            {
                var expectedTypeId = expectedType.Key.AsString();
                testOutputHelper.WriteLine("Type: " + expectedTypeId);
                var actualType = await parser.GetSchemaType(expectedTypeId);
                actualType.SchemaTypeId.FullId.Should().Be(expectedTypeId);

                var expectedProperties = expectedType.Value.AsMap("properties");
                VerifyProperties(expectedProperties, actualType.Properties.ToList());
            }
        }

        private void VerifyProperties(YamlMappingNode expectedProperties, List<Property> actualProperties,
            int level = 0)
        {
            var spacing1 = new string(' ', (level + 1) * 3);
            var spacing2 = new string(' ', (level + 2) * 3);
            expectedProperties.Children.Count.Should().Be(actualProperties.Count);
            foreach (var expectedProperty in expectedProperties)
            {
                // verify the property exists
                var expectedName = expectedProperty.Key.AsString();
                testOutputHelper.WriteLine(spacing1 + expectedName);
                var actualProperty = actualProperties.Single(p => p.Name == expectedName);

                // verify the type
                var expectedPropertyType = expectedProperty.Value.AsString("type");
                testOutputHelper.WriteLine(spacing2 + "Type: " + expectedPropertyType);
                expectedPropertyType.Should().Be(actualProperty.TypeId.FullId);

                // nested properties, if there are any
                var expectedNestedProperties = expectedProperty.Value.AsMap("properties");
                var isPrimitive = expectedNestedProperties.Children.Count == 0;
                if (isPrimitive)
                {
                    actualProperty.GetType().Should().Be(typeof(PropertyPrimitive));
                    var actualPrim = (PropertyPrimitive) actualProperty;

                    // verify secret
                    var expectedIsSecret = expectedProperty.Value.AsBoolean("is-secret");
                    testOutputHelper.WriteLine(spacing2 + "Secret: " + expectedIsSecret);
                    actualPrim.IsSecret.Should().Be(expectedIsSecret);
                    continue;
                }

                actualProperty.GetType().Should().Be(typeof(PropertyGroup));
                VerifyProperties(expectedNestedProperties, ((PropertyGroup) actualProperty).Properties.ToList(),
                    level + 1);
            }
        }
    }
}