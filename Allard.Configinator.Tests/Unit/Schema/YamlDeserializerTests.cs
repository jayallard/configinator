using System;
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
    public class YamlDeserializerTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public YamlDeserializerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Exhaustive()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "deserialization-test.yml");
            var yaml = await YamlUtility.GetYamlFromFile(file);
            var yamlStream = new YamlStream(yaml);

            var deserialized = YamlSchemaDeserializer
                .Deserialize(yamlStream.Documents)
                .ToList();

            deserialized.Count.Should().Be(2);
            
            var exhaustive = deserialized[0];
            exhaustive.Namespace.Should().Be("exhaustive");
            exhaustive.Properties.Count.Should().Be(3);
            exhaustive.Properties[0].PropertyName.Should().Be("my-kafka");
            exhaustive.Properties[0].TypeName.Should().Be("external/kafka");
            exhaustive.Properties[0].IsSecret.Should().Be(false);
            exhaustive.Properties[0].IsOptional.Should().Be(false);

            var other = deserialized[1];
            other.Secrets.Count.Should().Be(2);
            other.Optional.Count.Should().Be(1);
        }
    }
}