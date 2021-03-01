using System;
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
            const string path = "/a/b/c";
            var mem = new MemoryConfigStore();

            // write
            var config = new ConfigStoreValue(path, "a", "config");
            await mem.SetValueAsync(config);

            // read
            var read = await mem.GetValueAsync(path);
            read.Value.Should().Be("config");
            read.ETag.Should().NotBe("A");
        }

        [Fact]
        public async Task WriteFailsIfEtagChanges()
        {
            const string path = "/a/b/c";
            var mem = new MemoryConfigStore();

            // initialize
            var config = new ConfigStoreValue(path, "a", "config");
            await mem.SetValueAsync(config);

            // read
            var read1 = await mem.GetValueAsync(path);
            var read2 = await mem.GetValueAsync(path);

            read1 = read1.SetValue("write1");
            read2 = read2.SetValue("read2");

            // this will work
            await mem.SetValueAsync(read1);

            // this will fail because the etag in the repo
            // changed with the last write
            // so the etag on the write2 doesn't match what's in the repo.
            Func<Task> test = async () => await mem.SetValueAsync(read2);
            test.Should().Throw<Exception>()
                .WithMessage("Invalid etag - the value may have changed since the lst get.");
        }

        [Fact]
        public async Task EtagDoesntChangeIfNoChange()
        {
            const string path = "/a/b/c";
            var mem = new MemoryConfigStore();

            // initialize
            var config = new ConfigStoreValue(path, "A", "config");
            await mem.SetValueAsync(config);

            // read, then write.
            var read = await mem.GetValueAsync(path);
            await mem.SetValueAsync(read);

            // read again. see the etag is the same.
            var read2 = await mem.GetValueAsync(path);
            read2.ETag.Should().Be(read.ETag);
        }

        [Fact]
        public async Task EtagChangesIfValueChanges()
        {
            const string path = "/a/b/c";
            var mem = new MemoryConfigStore();

            // initialize
            var config = new ConfigStoreValue(path, "A", "config");
            await mem.SetValueAsync(config);

            // read, change the value, write
            var read = (await mem.GetValueAsync(path)).SetValue("blah blah blah");
            await mem.SetValueAsync(read);

            // read again. see the etag is the same.
            var read2 = await mem.GetValueAsync(path);
            read2.ETag.Should().NotBe(read.ETag);
        }
    }
}