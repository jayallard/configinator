using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Tests.Unit.Schema
{
    public class SchemaResolverTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SchemaResolverTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Blah()
        {
            var baseFolder = Path
                .Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "TestFiles", "Schemas", "TestTypes");

            // used to validate the results
            var expectedDto = (await GetDtos(Path.Combine(baseFolder, "ExpectedResolution"))).ToList();

            var actualDto = (await GetDtos(Path.Combine(baseFolder, "Types"))).ToList();
            var actuals = (await SchemaResolver.ConvertAsync(actualDto)).ToList();

            actuals.Count.Should().Be(expectedDto.Count);
            foreach (var expected in expectedDto)
            {
                // actual
                var expectedId = new SchemaTypeId(expected.TypeName);
                var actual =
                    actuals.Single(a => a.SchemaTypeId == expectedId);
                VerifyProperties(expected.Properties, actual.Properties.ToList());
            }


            testOutputHelper.WriteLine("");
        }

        private void VerifyProperties(IList<ModelDto.PropertyDto> expectedProperties,
            ICollection<Property> actual)
        {
            actual.Count.Should().Be(expectedProperties.Count);
            foreach (var expectedProperty in expectedProperties)
            {
                var actualProperty = actual.Single(a => a.Name == expectedProperty.PropertyName);
                actualProperty.IsOptional.Should().Be(expectedProperty.IsOptional);
                actualProperty.TypeId.Should().Be(new SchemaTypeId(expectedProperty.TypeName));
                if (expectedProperty.Properties.Count == 0)
                {
                    // primitive
                    actualProperty.Should().BeOfType<PropertyPrimitive>();
                    var prim = (PropertyPrimitive) actualProperty;
                    prim.IsSecret.Should().Be(expectedProperty.IsSecret);
                    continue;
                }

                // group
                actualProperty.Should().BeOfType<PropertyGroup>();
                var group = (PropertyGroup) actualProperty;
                VerifyProperties(expectedProperty.Properties, group.Properties);
            }
        }

        private static async Task<IEnumerable<ModelDto.TypeDto>> GetDtos(string folder)
        {
            var repo = new FileSchemaRepository(folder);
            return await repo.GetSchemaTypes();

            // var yamlTasks = Directory
            //     .GetFiles(folder)
            //     .Select(async f => await YamlUtility.GetYamlFromFile(f));
            // var yaml = await Task.WhenAll(yamlTasks);
            // return yaml
            //     .SelectMany(YamlSchemaDeserializer.Deserialize)
            //     .ToList();
        }
    }
}