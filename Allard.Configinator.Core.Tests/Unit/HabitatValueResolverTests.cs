using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class HabitatValueResolverTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public HabitatValueResolverTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task LoadChildHabitats()
        {
            // h1 : h2a
            // h1 : h2b : h3a 
            var h1 = new DummyHabitat {HabitatId = new HabitatId("h1")};
            var h2A = new DummyHabitat {HabitatId = new HabitatId("h2a"),BaseHabitat = h1};
            h1.ChildHabitats.Add(h2A);

            var h2B = new DummyHabitat {HabitatId = new HabitatId("h2b"), BaseHabitat = h1};
            h1.ChildHabitats.Add(h2B);

            var h3A = new DummyHabitat {HabitatId = new HabitatId("h3a"),BaseHabitat = h2B};
            h2B.ChildHabitats.Add(h3A);

            static Task<ObjectDto> ConfigStore(IHabitat h) => Task.FromResult(new ObjectDto());

            var model = new ObjectDto();
            var resolver = new HabitatValueResolver(model, ConfigStore, h1);
            await resolver.LoadExistingValues();
            resolver.VersionedHabitats.Count().Should().Be(4);
        }

        [Fact]
        public async Task CopyDown()
        {
            var model = new ObjectDto().AddString("hello");
            
            // value = starts as null, but we will set it to world
            // after load.
            var h1Value = new ObjectDto().AddString("hello");
            
            // no value, so inherit world.
            var h2AValue = new ObjectDto();

            // explicit value, so will keep it.
            var h2BValue = new ObjectDto().AddString("hello", "galaxy");
            
            // null value, so inherit galaxy./
            var h3AValue = new ObjectDto().AddString("hello");
            
            // h1 : h2a
            // h1 : h2b : h3a 
            var h1 = new DummyHabitat {HabitatId = new HabitatId("h1")};
            var h2A = new DummyHabitat {HabitatId = new HabitatId("h2a"),BaseHabitat = h1};
            h1.ChildHabitats.Add(h2A);

            var h2B = new DummyHabitat {HabitatId = new HabitatId("h2b"), BaseHabitat = h1};
            h1.ChildHabitats.Add(h2B);

            var h3A = new DummyHabitat {HabitatId = new HabitatId("h3a"),BaseHabitat = h2B};
            h2B.ChildHabitats.Add(h3A);

            var configValues = new Dictionary<IHabitat, ObjectDto>
            {
                {h1, h1Value},
                {h2A, h2AValue},
                {h2B, h2BValue},
                {h3A, h3AValue}
            };

            Task<ObjectDto> ConfigStore(IHabitat h) => Task.FromResult(configValues[h]);

            var resolver = new HabitatValueResolver(model, ConfigStore, h1);
            
            // this loads all the values, but hasn't done any work yet.
            await resolver.LoadExistingValues();
            
            // overwrite the value  
            h1Value.GetProperty("hello").SetValue("world");
            resolver.OverwriteValue(h1, h1Value);

            var resolved = resolver.VersionedHabitats.ToDictionary(v => v.Name);
            
            // h1 has a value of world, as it was assigned.
            resolved["h1"].Versions.Last().Properties.Single().Value.Should().Be("world");
            
            // h2a wasn't given a value, so it inherits WORLD
            resolved["h2a"].Versions.Last().Properties.Single().Value.Should().Be("world");
            
            // h2b was explicitly set
            resolved["h2b"].Versions.Last().Properties.Single().Value.Should().Be("galaxy");

            // h3a wasn't set, so inherits galaxy from h2b
            resolved["h3a"].Versions.Last().Properties.Single().Value.Should().Be("galaxy");
        }
    }

    [DebuggerDisplay("Id={HabitatId.Id}")]
    public class DummyHabitat : IHabitat
    {
        public List<IHabitat> ChildHabitats { get; } = new();
        
        public IRealm Realm { get; set; }
        public HabitatId HabitatId { get; set;  }
        public IHabitat BaseHabitat { get; set;  }
        public IEnumerable<IHabitat> Children => ChildHabitats;
    }
}