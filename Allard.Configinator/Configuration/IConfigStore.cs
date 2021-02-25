using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
{
    /// <summary>
    ///     Read and write configuration to a
    ///     backing store, such as Vault, Aws Secrets manager,
    ///     etc.
    /// </summary>
    public interface IConfigStore
    {
        Task<ConfigurationValue> GetValueAsync(string path);
        Task SetValueAsync(ConfigurationValue value);
    }
}