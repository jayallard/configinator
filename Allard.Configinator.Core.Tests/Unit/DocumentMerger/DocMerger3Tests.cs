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

            var only = (await DocMerger3.Merge(model, a, b, c));
                var x = only.Objects.Single();
            only.Objects.Count.Should().Be(0);
            only.Properties.Count.Should().Be(1);
            only.Properties.Single().Path.Should().Be("/a");
            only.Properties.Single().Value.Should().Be("d");
            only.Properties.Single().Layers.Select(l => l.Transition)
                .Should().AllBeEquivalentTo(Transition.Set);
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
            var merged = (await DocMerger3.Merge(model, a, b, c, d, e));
            merged.Properties.Single().Value.Should().Be("planet");
            merged.Properties.Single().Layers[0].Transition.Should().Be(Transition.Set);
            merged.Properties.Single().Layers[0].Value.Should().Be("world");

            merged.Properties.Single().Layers[1].Transition.Should().Be(Transition.Delete);
            merged.Properties.Single().Layers[1].Value.Should().BeNull();

            merged.Properties.Single().Layers[2].Transition.Should().Be(Transition.DoesntExist);
            merged.Properties.Single().Layers[2].Value.Should().BeNull();

            merged.Properties.Single().Layers[3].Transition.Should().Be(Transition.Set);
            merged.Properties.Single().Layers[3].Value.Should().Be("planet");

            merged.Properties.Single().Layers[4].Transition.Should().Be(Transition.Inherit);
            merged.Properties.Single().Layers[4].Value.Should().Be("planet");
        }

        [Fact]
        public async Task SetToSameValue()
        {
            var model = JsonDocument.Parse("{ \"hello\": \"\" }");
            var a = JsonDocument.Parse("{ \"hello\": \"world\" }");
            var b = JsonDocument.Parse("{ \"hello\": \"world\" }");
            var merge = await DocMerger3.Merge(model, a, b);
            merge.Properties.Single().Layers.Last().Transition.Should().Be(Transition.SetToSameValue);
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
            var merged = (await DocMerger3.Merge(model, a, b, c, d));

            // hello and stuff
            merged.Properties.Count.Should().Be(2);
            var hello = merged.Properties.Single(m => m.Name == "hello");
            hello.Value.Should().Be("bob");
            hello.Layers[0].Transition.Should().Be(Transition.Set);
            hello.Layers[1].Transition.Should().Be(Transition.Inherit);
            hello.Layers[2].Transition.Should().Be(Transition.Inherit);
            hello.Layers[3].Transition.Should().Be(Transition.Set);

            var stuff = merged.Objects.Single(m => m.Name == "stuff");
            stuff.Properties.Count.Should().Be(1);

            var stuff2 = stuff.Objects.Single(c => c.Name == "stuff2");
            stuff2.Properties.Count.Should().Be(1);
            var hi = stuff2.Properties.Single(c => c.Name == "hi");
            hi.Value.Should().Be("bye");

            testOutputHelper.WriteLine(merged.ToJsonString());
        }
    }
}