using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.DocumentMerger
{
    public class DocMerger3Tests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DocMerger3Tests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task LastOneWins()
        {
            var model = JsonDocument.Parse("{ \"a\": \"\" }");
            var a = JsonDocument.Parse("{ \"a\": \"b\" }");
            var b = JsonDocument.Parse("{ \"a\": \"c\" }");
            var c = JsonDocument.Parse("{ \"a\": \"d\" }");

            var only = (await DocMerger3.Merge(model, a, b, c)).Single();
            only.Children.Count.Should().Be(0);
            only.Path.Should().Be("/a");
            only.Property.Value.Should().Be("d");
            only.Property.Layers.Select(l => l.Transition).Should().AllBeEquivalentTo(Transition.Set);
        }

        [Fact]
        public async Task MessWithProperty()
        {
            var model = JsonDocument.Parse("{ \"test\": \"\" }");
            var a = JsonDocument.Parse("{ \"test\": \"world\" }");
            var b = JsonDocument.Parse("{ \"test\": null }");
            var c = JsonDocument.Parse("{ }");
            var d = JsonDocument.Parse("{ \"test\": \"planet\" }");
            var e = JsonDocument.Parse("{ }");
            var merged = (await DocMerger3.Merge(model, a, b, c, d, e)).Single();
            merged.Property.Value.Should().Be("planet");
            merged.Property.Layers[0].Transition.Should().Be(Transition.Set);
            merged.Property.Layers[0].Value.Should().Be("world");

            merged.Property.Layers[1].Transition.Should().Be(Transition.Delete);
            merged.Property.Layers[1].Value.Should().BeNull();

            merged.Property.Layers[2].Transition.Should().Be(Transition.DoesntExist);
            merged.Property.Layers[2].Value.Should().BeNull();

            merged.Property.Layers[3].Transition.Should().Be(Transition.Set);
            merged.Property.Layers[3].Value.Should().Be("planet");

            merged.Property.Layers[4].Transition.Should().Be(Transition.Inherit);
            merged.Property.Layers[4].Value.Should().Be("planet");
        }

        [Fact]
        public async Task SetToSameValue()
        {
            var model = JsonDocument.Parse("{ \"hello\": \"\" }");
            var a = JsonDocument.Parse("{ \"hello\": \"world\" }");
            var b = JsonDocument.Parse("{ \"hello\": \"world\" }");
            var merge = await DocMerger3.Merge(model, a, b);
            merge.Single().Property.Layers.Last().Transition.Should().Be(Transition.SetToSameValue);
        }

        [Fact]
        public async Task MultiLayer()
        {
            // { hello: '', stuff: { stuff2: { hi: '' } } }
            var model = JsonDocument.Parse("{ \"hello\": \"\", \"stuff\": { \"stuff2\": { \"hi\": \"\" }}}");
            var a = JsonDocument.Parse("{ \"hello\": \"world\" }");
            var b = JsonDocument.Parse("{                       \"stuff\": { \"stuff2\": { \"hi\": \"there\" }}}");
            var c = JsonDocument.Parse("{                       \"stuff\": { \"stuff2\": { \"hi\": \"bye\" }}}");
            var d = JsonDocument.Parse("{ \"hello\": \"bob\",   \"stuff\": { \"stuff2\": { \"hi\": \"bye\" }}}");
            var merged = (await DocMerger3.Merge(model, a, b, c, d)).ToList();

            // hello and stuff
            merged.Count.Should().Be(2);
            var hello = merged.Single(m => m.Property.Name == "hello");
            hello.Property.Value.Should().Be("bob");
            hello.Property.Layers[0].Transition.Should().Be(Transition.Set);
            hello.Property.Layers[1].Transition.Should().Be(Transition.Inherit);
            hello.Property.Layers[2].Transition.Should().Be(Transition.Inherit);
            hello.Property.Layers[3].Transition.Should().Be(Transition.Set);

            var stuff = merged.Single(m => m.Property.Name == "stuff");
            stuff.Children.Count.Should().Be(1);

            var stuff2 = stuff.Children.Single(c => c.Property.Name == "stuff2");
            stuff2.Children.Count.Should().Be(1);
            var hi = stuff2.Children.Single(c => c.Property.Name == "hi");
            hi.Property.Value.Should().Be("bye");

            testOutputHelper.WriteLine(merged.ToJsonString());
        }
    }
}