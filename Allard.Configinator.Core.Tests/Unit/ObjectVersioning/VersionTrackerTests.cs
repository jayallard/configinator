using System.Collections.Generic;
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
        public void Blah()
        {
            var model = new ObjectDto
            {
                Name = "root",
                Objects = new List<ObjectDto>
                {
                    new()
                    {
                        Name = "hello",
                        Properties = new List<PropertyDto>
                        {
                            new() {Name = "World"},
                            new() {Name = "Galaxy"}
                        }
                    }
                },
                Properties = new List<PropertyDto>
                {
                    new() {Name = "a"},
                    new() {Name = "b"},
                    new() {Name = "c"},
                }
            };

            var a = model.Clone();
            a.GetProperty("a").SetValue("aaa 1");
            a.GetProperty("b").SetValue("bbb 1");
            a.GetProperty("c").SetValue("ccc 1");

            var b = model.Clone();
            b.GetProperty("a").SetValue("aaa 2");
            b.GetProperty("c").SetValue("ccc 2");

            
            var tracker = new VersionTracker(model);
            tracker.Add("a", a);
            tracker.Add("b", b);
            
            testOutputHelper.WriteLine("");
        }
    }
}