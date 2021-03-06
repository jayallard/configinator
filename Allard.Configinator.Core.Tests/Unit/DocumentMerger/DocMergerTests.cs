using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.DocumentMerger
{
    public class DocMergerTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DocMergerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public async Task DeleteProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": null }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var result = (await DocMerger.Merge(merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().BeNull();
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.Delete);
        }
        
        [Fact]
        public async Task DeleteThenAddBack()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            var middle = JsonDocument.Parse("{ \"test\": null }").RootElement;
            var middle2 = JsonDocument.Parse("{  }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": \"planet\" }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("middle", 1, new JsonObjectNode("", middle)),
                new("middle2", 2, new JsonObjectNode("", middle2)),
                new("bottom", 3, new JsonObjectNode("", bottom))
            };

            var result = (await DocMerger.Merge(merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History.Count.Should().Be(4);
            prop.Value.Should().Be("planet");
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.Delete);
            prop.History[2].Transition.Should().Be(Transition.DoesntExist);
            prop.History[3].Transition.Should().Be(Transition.Set);
        }
        
        /// <summary>
        /// Doc 0 doesn't have the property.
        /// Doc 1 does.
        /// Back fill the history for doc 0 with DoesntExist.
        /// </summary>
        [Fact]
        public async Task PropertySetInSecondDocument()
        {
            var top = JsonDocument.Parse("{  }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var result = (await DocMerger.Merge(merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.History[0].Transition.Should().Be(Transition.DoesntExist);
            prop.History[1].Transition.Should().Be(Transition.Set);
        }


        [Fact]
        public async Task InheritProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            // will inherit test=world
            var bottom = JsonDocument.Parse("{  }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var result = (await DocMerger.Merge(merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.Inherit);
        }

        /// <summary>
        /// If doc 1 = hello: world,
        /// And doc 2 = hello: world,
        /// then the doc2 transition should be SetToSameValue.
        /// During load, it is set to Set.
        /// The cleanup reset it to SetToSameValue.
        /// </summary>
        [Fact]
        public async Task SetToSameValue()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            // will inherit test=world
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode(string.Empty, top)),
                new("bottom", 1, new JsonObjectNode(string.Empty, bottom))
            };

            var result = (await DocMerger.Merge(merge))
                .ToDictionary(m => m.Path, m => m.Property);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.SetToSameValue);
        }
    }
}