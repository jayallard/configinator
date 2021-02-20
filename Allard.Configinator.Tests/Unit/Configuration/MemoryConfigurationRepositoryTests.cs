using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Tests.Unit.Configuration
{
    public class MemoryConfigurationRepositoryTests
    {
        [Fact]
        public async Task WriteRead()
        {
            var mem = new MemoryConfigStore();

            var configId = new ConfigurationSectionId("WriteReadTest", "blah");
            var configSection = new ConfigurationSection(configId, "path", null, null);
            var space = new Habitat("name", "description", new HashSet<string>());

            // write
            var config = new ConfigurationSectionValue(space, configSection, "A", "config");
            await mem.SetValueAsync(config);

            // read
            var read = await mem.GetValue(space, configSection);
            read.Value.Should().Be("config");
            read.ETag.Should().NotBe("A");
        }

        [Fact]
        public async Task WriteFailsIfEtagChanges()
        {
            var mem = new MemoryConfigStore();

            var configId = new ConfigurationSectionId("WriteReadTest", "blah");
            var configSection = new ConfigurationSection(configId, "path", null, null);
            var space = new Habitat("name", "description", new HashSet<string>());

            // initialize
            var config = new ConfigurationSectionValue(space, configSection, "A", "config");
            await mem.SetValueAsync(config);

            // read
            var read1 = await mem.GetValue(space, configSection);
            var read2 = await mem.GetValue(space, configSection);

            read1.SetValue("write1");
            read2.SetValue("read2");

            // this will work
            await mem.SetValueAsync(read1);

            // this will fail because the etag in the repo
            // changed with the last write
            // so the etag on the write2 doesn't match what's in the repo.
            Func<Task> test = async () => await mem.SetValueAsync(read2);
            test.Should().Throw<Exception>().WithMessage("etag change");
        }

        [Fact]
        public async Task EtagDoesntChangeIfNoChange()
        {
            var mem = new MemoryConfigStore();

            var configId = new ConfigurationSectionId("EtagDoesntChangeIfNoChange", "blah");
            var configSection = new ConfigurationSection(configId, "path", null, null);
            var space = new Habitat("name", "description", new HashSet<string>());

            // initialize
            var config = new ConfigurationSectionValue(space, configSection, "A", "config");
            await mem.SetValueAsync(config);

            // read, then write.
            var read = await mem.GetValue(space, configSection);
            await mem.SetValueAsync(read);

            // read again. see the etag is the same.
            var read2 = await mem.GetValue(space, configSection);
            read2.ETag.Should().Be(read.ETag);
        }

        [Fact]
        public async Task EtagChangesIfValueChanges()
        {
            var mem = new MemoryConfigStore();

            var configId = new ConfigurationSectionId("EtagChangesIfValueChanges", "blah");
            var configSection = new ConfigurationSection(configId, "path", null, null);
            var habitat = new Habitat("name", "description", new HashSet<string>());

            // initialize
            var config = new ConfigurationSectionValue(habitat, configSection, "A", "config");
            await mem.SetValueAsync(config);

            // read, change the value, write
            var read = (await mem.GetValue(habitat, configSection)).SetValue("blah blah blah");
            await mem.SetValueAsync(read);

            // read again. see the etag is the same.
            var read2 = await mem.GetValue(habitat, configSection);
            read2.ETag.Should().NotBe(read.ETag);
        }
    }
}