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
    public class SchemaLoaderTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SchemaLoaderTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Blah()
        {
            var baseFolder = Path
                .Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "TestFiles", "Schemas", "TestTypes");

            var expectedDtos = await GetDtos(Path.Combine(baseFolder, "ExpectedResolution"));
            var actualDtos = await GetDtos(Path.Combine(baseFolder, "Types"));

            actualDtos.Count().Should().Be(expectedDtos.Count());


            testOutputHelper.WriteLine("");
        }

        private static async Task<IEnumerable<ModelDto.TypeDto>> GetDtos(string folder)
        {
            var yamlTasks = Directory
                .GetFiles(folder)
                .Select(async f => await YamlUtility.GetYamlFromFile(f));
            var yaml = await Task.WhenAll(yamlTasks);
            return yaml
                .SelectMany(YamlSchemaDeserializer.Deserialize)
                .ToList();
        }
    }
}