using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Allard.Configinator.AwsSecretsManager
{
    public class AwsSecretsManagerConfigStore : IConfigStore
    {
        private readonly IAmazonSecretsManager client;

        public AwsSecretsManagerConfigStore(AmazonSecretsManagerConfig config = null,
            AWSCredentials credentials = null)
        {
            // based on the values provide,
            // use the correct constructor.
            // either both, none, or one or the other.
            // don't know if this is necessary or if the constructor will ignore nulls for us.
            // should try that.


            // neither
            if (config == null && credentials == null)
            {
                client = new AmazonSecretsManagerClient();
                return;
            }

            // both
            if (config != null && credentials != null)
            {
                client = new AmazonSecretsManagerClient(credentials, config);
                return;
            }

            // one or the other
            client =
                credentials == null
                    ? new AmazonSecretsManagerClient(config)
                    : new AmazonSecretsManagerClient(credentials);
        }


        public async Task<ConfigStoreValue> GetValueAsync(string path)
        {
            var request = new GetSecretValueRequest {SecretId = path};
            var value = await client.GetSecretValueAsync(request);

            // todo: figure out what to use for etag. does ARN change for each version?
            return new ConfigStoreValue(path, value.VersionId, value.SecretString);
        }

        public async Task<ConfigStoreValue> SetValueAsync(ConfigStoreValue value)
        {
            if (value.ETag == null)
            {
                var createRequest = new CreateSecretRequest {Name = value.Path, SecretString = value.Value};
                var createResponse = await client.CreateSecretAsync(createRequest);
                return value with {ETag = createResponse.VersionId};
            }

            var updateRequest = new UpdateSecretRequest {SecretId = value.Path, SecretString = value.Value};
            var updateResponse = await client.UpdateSecretAsync(updateRequest);
            return value with {ETag = updateResponse.VersionId};
        }

        public async Task DeleteValueAsync(ConfigStoreValue value)
        {
            var deleteRequest = new DeleteSecretRequest
                {ForceDeleteWithoutRecovery = true, SecretId = value.Path};
            var deleteResponse = await client.DeleteSecretAsync(deleteRequest);
        }
    }
}