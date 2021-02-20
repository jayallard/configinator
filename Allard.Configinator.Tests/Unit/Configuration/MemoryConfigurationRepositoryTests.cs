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
        private readonly Habitat habitat = new Habitat("test", "test", new HashSet<string>());
        
        [Fact]
        public async Task WriteRead()
        {
            var mem = new MemoryConfigStore();

            var configId = new ConfigurationSectionId("WriteReadTest", "blah");
            var configSection = new ConfigurationSection(configId, "path", null, null);
            var space = new Habitat("name", "description", new HashSet<string>());
            
            // write
            var config = new ConfigurationSectionValue(space, configSection, "A", "config");
            await mem.WriteConfiguration(config);

            // read
            var read = await mem.GetConfiguration(space, configSection);
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
            await mem.WriteConfiguration(config);

            // read
            var read1 = await mem.GetConfiguration(space, configSection);
            var read2 = await mem.GetConfiguration(space, configSection);
            read1.Should().Be(read2);

            var write1 = read1 with {Value = "write1"};
            var write2 = read2 with {Value = "write2"};

            // this will work
            await mem.WriteConfiguration(write1);

            // this will fail because the etag in the repo
            // changed with the last write
            // so the etag on the write2 doesn't match what's in the repo.
            Func<Task> test = async () => await mem.WriteConfiguration(write2);
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
            await mem.WriteConfiguration(config);

            // read, then write.
            var read = await mem.GetConfiguration(space, configSection);
            await mem.WriteConfiguration(read);

            // read again. see the etag is the same.
            var read2 = await mem.GetConfiguration(space, configSection);
            read2.ETag.Should().Be(read.ETag);
        }

        [Fact]
        public async Task EtagChangesIfValueChanges()
        {
            var mem = new MemoryConfigStore();

            var configId = new ConfigurationSectionId("EtagChangesIfValueChanges", "blah");
            var configSection = new ConfigurationSection(configId, "path", null, null);
            var space = new Habitat("name", "description", new HashSet<string>());

            // initialize
            var config = new ConfigurationSectionValue(space, configSection, "A", "config");
            await mem.WriteConfiguration(config);

            // read, change the value, write
            var read = await mem.GetConfiguration(space, configSection) with {Value = "blah blah blah"};
            await mem.WriteConfiguration(read);

            // read again. see the etag is the same.
            var read2 = await mem.GetConfiguration(space, configSection);
            read2.ETag.Should().NotBe(read.ETag);
        }
    }
}