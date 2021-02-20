using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Tests.Unit.Configuration
{
    public class SpaceDeserializerTests
    {
        [Fact]
        public async Task Deserialize()
        {
            var yaml = (await YamlUtility.GetYamlFromFile("TestFiles", "Spaces", "good.yml"))
                .Single()
                .RootNode;
            var spaces = Deserializers
                .DeserializeSpace(yaml.AsMap())
                .ToDictionary(s => s.Name);
            spaces.Count.Should().Be(4);

            spaces["production"].Bases.Should().BeEmpty();
            spaces["production"].Description.Should().BeNull();

            spaces["development"].Bases.Should().BeEmpty();
            spaces["development"].Description.Should().BeNull();

            spaces["dev-jay1"].Description.Should().Be("jay1");
            spaces["dev-jay1"].Bases.Count.Should().Be(1);

            spaces["dev-jay2"].Description.Should().BeNull();
            spaces["dev-jay2"].Bases.Count.Should().Be(2);
        }
    }
}