using System;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Amazon.SecretsManager.Model;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.AwsSecretsManager.Tests.Integration
{
    public class AwsSecretsManagerConfigStoreTests
    {
        public AwsSecretsManagerConfigStoreTests(ITestOutputHelper writer)
        {
            this.writer = writer;
        }

        private readonly ITestOutputHelper writer;

        [Fact(Skip = "tool - use as needed")]
        public async Task Cleanup()
        {
            var client = new AwsSecretsManagerConfigStore();
            await client.DeleteValueAsync(new ConfigStoreValue("test1", "blah", "blah"));
        }

        [Fact]
        public async Task Crud()
        {
            var client = new AwsSecretsManagerConfigStore();
            var path = "test/" + Guid.NewGuid();
            var value1 = Guid.NewGuid().ToString();

            // create
            var value1Response = await client.SetValueAsync(new ConfigStoreValue(path, null, value1));
            value1Response.ETag.Should().NotBeNull();

            // get
            var getValue1 = await client.GetValueAsync(path);
            getValue1.Value.Should().Be(value1);

            // update
            var value2 = Guid.NewGuid().ToString();
            var update = new ConfigStoreValue(path, value1Response.ETag, value2);
            await client.SetValueAsync(update);
            
            // get
            var getValue2 = await client.GetValueAsync(path);
            getValue2.Value.Should().Be(value2);
            
            // delete
            await client.DeleteValueAsync(getValue2);
            Func<Task> getValue3 = () => client.GetValueAsync(path);
            getValue3.Should().Throw<InvalidRequestException>()
                // this might be fragile. how long between FLAGGED FOR DELETE and DELETED?
                // if it's already gone, then the message will be different.
                // assert a different way.
                .WithMessage("You can't perform this operation on the secret because it was marked for deletion.");
        }
    }
}