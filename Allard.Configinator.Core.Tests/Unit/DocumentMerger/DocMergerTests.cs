using System.Collections.Generic;
using System.Text.Json;
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
        public void DeleteProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": null }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var node = new JsonObjectNode(string.Empty, top);
            var result = new DocMerger(merge).Merge();
            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().BeNull();
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.Delete);

            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
        }

        /// <summary>
        /// Doc 0 doesn't have the property.
        /// Doc 1 does.
        /// Back fill the history for doc 0 with DoesntExist.
        /// </summary>
        [Fact]
        public void PropertySetInSecondDocument()
        {
            var top = JsonDocument.Parse("{  }").RootElement;
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var result = new DocMerger(merge).Merge();
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.History[0].Transition.Should().Be(Transition.DoesntExist);
            prop.History[1].Transition.Should().Be(Transition.Set);
        }


        [Fact]
        public void InheritProperty()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            // will inherit test=world
            var bottom = JsonDocument.Parse("{  }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode("", top)),
                new("bottom", 1, new JsonObjectNode("", bottom))
            };

            var result = new DocMerger(merge).Merge();
            var prop = result["/test"];
            prop.History.Count.Should().Be(2);
            prop.Value.Should().Be("world");
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.Inherit);
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));
        }

        /// <summary>
        /// If doc 1 = hello: world,
        /// And doc 2 = hello: world,
        /// then the doc2 transition should be SetToSameValue.
        /// During load, it is set to SET.
        /// The cleanup reset it to SETTOSAMEVALUE.
        /// </summary>
        [Fact]
        public void SetToSameValue()
        {
            var top = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;
            // will inherit test=world
            var bottom = JsonDocument.Parse("{ \"test\": \"world\" }").RootElement;

            var merge = new List<DocumentToMerge>
            {
                new("top", 0, new JsonObjectNode(string.Empty, top)),
                new("bottom", 1, new JsonObjectNode(string.Empty, bottom))
            };

            var result = new DocMerger(merge).Merge();
            testOutputHelper.WriteLine(JsonSerializer.Serialize(result));

            var prop = result["/test"];
            prop.History[0].Transition.Should().Be(Transition.Set);
            prop.History[1].Transition.Should().Be(Transition.SetToSameValue);
        }
    }
}