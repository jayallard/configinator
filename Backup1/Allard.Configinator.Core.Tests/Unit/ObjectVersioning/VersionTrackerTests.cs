using System.Linq;
using System.Text;
using Allard.Configinator.Core.ObjectVersioning;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.ObjectVersioning
{
    public class VersionTrackerTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public VersionTrackerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Demo()
        {
            var model = new Node()
                .SetName("root")
                .Add(new Node()
                    .SetName("hello")
                    .AddString("world")
                    .AddString("Galaxy"))
                .AddString("a")
                .AddString("b")
                .AddString("c");

            var a = model.Clone();
            a.GetProperty("a").SetValue("aaa 1");
            a.GetProperty("b").SetValue("bbb 1");
            a.GetProperty("c").SetValue("ccc 1");
            a.GetObject("hello").GetProperty("Galaxy").SetValue("milky way");

            var b = model.Clone();
            b.GetProperty("a").SetValue("aaa 2");
            b.GetProperty("c").SetValue("ccc 2");
            b.GetObject("hello").GetProperty("Galaxy").SetValue("milky street");

            var tracker = new VersionTracker(model);
            tracker.AddVersion("a", a);
            tracker.AddVersion("b", b);

            var output = new StringBuilder();
            PrintObject(output, tracker.Versions.First(), 0);
            testOutputHelper.WriteLine(output.ToString());
        }

        private static void PrintObject(StringBuilder output, VersionedNode obj, int level)
        {
            var spaces = new string('\t', level);
            var spaces2 = new string('\t', level + 1);
            output
                .Append(spaces)
                .Append("Object: ")
                .AppendLine(obj.Name);
            foreach (var p in obj.Properties)
            {
                output
                    .Append(spaces2)
                    .Append("Property: ")
                    .AppendLine(p.Name);
                var current = p;
                while (current != null)
                {
                    output
                        .Append(spaces2)
                        .Append("\tVersion ")
                        .Append(current.VersionName)
                        .Append(": ")
                        .AppendLine(current.Value);
                    current = current.NextVersion;
                }
            }

            foreach (var o in obj.Objects) PrintObject(output, o, level + 1);
        }
    }
}