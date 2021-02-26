using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Habitats;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Tests.Unit.Configuration
{
    public class HabitatDeserializerTests
    {
        [Fact]
        public async Task Deserialize()
        {
            var yaml = (await YamlUtility.GetYamlFromFile("TestFiles", "Habitats", "good.yml"))
                .Single()
                .RootNode;
            var habitats = HabitatYamlDeserializer
                .Deserialize(yaml.AsMap())
                .ToDictionary(s => s.Name);
            habitats.Count.Should().Be(4);

            habitats["production"].Bases.Should().BeEmpty();
            habitats["production"].Description.Should().BeNull();

            habitats["development"].Bases.Should().BeEmpty();
            habitats["development"].Description.Should().BeNull();

            habitats["dev-jay1"].Description.Should().Be("jay1");
            habitats["dev-jay1"].Bases.Count.Should().Be(1);

            habitats["dev-jay2"].Description.Should().BeNull();
            habitats["dev-jay2"].Bases.Count.Should().Be(2);
        }
    }
}