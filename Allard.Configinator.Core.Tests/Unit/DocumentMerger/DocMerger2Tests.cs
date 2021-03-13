using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.DocumentMerger
{
    public class DocMerger2Tests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DocMerger2Tests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Flatten()
        {
            var json =
                "{ \"hello\": { \"a\": { \"this\": \"stupid\", \"nest\": { \"bird\": \"eagle\" } } }, \"greetings\": \"planet\" }";
            var doc = JsonDocument.Parse(json);
            var stuff = DocMerger2.Flatten(doc, false);
            testOutputHelper.WriteLine("");
        }

        [Fact]
        public async Task ToJsonString()
        {
            var a = JsonDocument.Parse(
                "{ \"hello\": \"world\", \"santa\": { \"job\": \"slacker\", \"marital-status\": \"married\", \"favorite-color\": \"red\" } }");

            // change HELLO to PLANET
            // change santa/marital-status to divorced.
            // delete favorite-color
            // add blah/do=something
            var b = JsonDocument.Parse(
                "{ \"hello\": \"planet\", \"santa\": { \"job\": \"slacker\", \"marital-status\": \"divorced\" , \"favorite-color\": null},  \"blah\": { \"do\": \"something\" } }");

            var merge = new List<DocumentToMerge>
            {
                new("top", a),
                new("bottom", b)
            };

            var result = (await DocMerger2.Merge(b, merge)).ToJsonString();
            var resultDoc = JsonDocument.Parse(result).RootElement;
            resultDoc.GetProperty("hello").GetString().Should().Be("planet");
            resultDoc.GetProperty("blah").GetProperty("do").GetString().Should().Be("something");
            var santa = resultDoc.GetProperty("santa");
            santa.GetProperty("marital-status").GetString().Should().Be("divorced");
            santa.GetProperty("job").GetString().Should().Be("slacker");
            santa.GetProperty("favorite-color").GetString().Should().BeNull();
        }

        [Fact]
        public async Task DeleteProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }");
            var bottom = JsonDocument.Parse("{ \"test\": null }");

            var merge = new List<DocumentToMerge>
            {
                new("top", top),
                new("bottom", bottom)
            };

            var result = (await DocMerger2.Merge(top, merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.Layers.Count.Should().Be(2);
            prop.Value.Should().BeNull();
            prop.Layers[0].Transition.Should().Be(Transition.Set);
            prop.Layers[1].Transition.Should().Be(Transition.Delete);
        }

        [Fact]
        public async Task DeleteThenAddBack()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }");
            var middle = JsonDocument.Parse("{ \"test\": null }");
            var middle2 = JsonDocument.Parse("{  }");
            var bottom = JsonDocument.Parse("{ \"test\": \"planet\" }");

            var merge = new List<DocumentToMerge>
            {
                new("top", top),
                new("middle", middle),
                new("middle2", middle2),
                new("bottom", bottom)
            };

            var result = (await DocMerger2.Merge(top, merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.Layers.Count.Should().Be(4);
            prop.Value.Should().Be("planet");
            prop.Layers[0].Transition.Should().Be(Transition.Set);
            prop.Layers[1].Transition.Should().Be(Transition.Delete);
            prop.Layers[2].Transition.Should().Be(Transition.DoesntExist);
            prop.Layers[3].Transition.Should().Be(Transition.Set);
        }

        /// <summary>
        ///     Doc 0 doesn't have the property.
        ///     Doc 1 does.
        ///     Back fill the history for doc 0 with DoesntExist.
        /// </summary>
        [Fact]
        public async Task PropertySetInSecondDocument()
        {
            var top = JsonDocument.Parse("{  }");
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }");

            var merge = new List<DocumentToMerge>
            {
                new("top", top),
                new("bottom", bottom)
            };

            var result = (await DocMerger2.Merge(bottom, merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.Layers.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.Layers[0].Transition.Should().Be(Transition.DoesntExist);
            prop.Layers[1].Transition.Should().Be(Transition.Set);
        }


        [Fact]
        public async Task InheritProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }");
            // will inherit test=world
            var bottom = JsonDocument.Parse("{  }");

            var merge = new List<DocumentToMerge>
            {
                new("top", top),
                new("bottom", bottom)
            };

            var result = (await DocMerger2.Merge(top, merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
            var prop = result["/test"];
            prop.Layers.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.Layers[0].Transition.Should().Be(Transition.Set);
            prop.Layers[1].Transition.Should().Be(Transition.Inherit);
        }

        /// <summary>
        ///     If doc 1 = hello: world,
        ///     And doc 2 = hello: world,
        ///     then the doc2 transition should be SetToSameValue.
        ///     During load, it is set to Set.
        ///     The cleanup reset it to SetToSameValue.
        /// </summary>
        [Fact]
        public async Task SetToSameValue()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }");
            // will inherit test=world
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }");

            var merge = new List<DocumentToMerge>
            {
                new("top", top),
                new("bottom", bottom)
            };

            var result = (await DocMerger2.Merge(top, merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.Layers[0].Transition.Should().Be(Transition.Set);
            prop.Layers[1].Transition.Should().Be(Transition.SetToSameValue);
        }
    }
}