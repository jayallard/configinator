using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Allard.Configinator.Core.Infrastructure
{
    /// <summary>
    ///     For demo/test purposes only.
    ///     Uses a dictionary as a configuration store.
    ///     The assumption is that any backing store will reject
    ///     requests if the hash doesn't match. Thus, it really
    ///     is the responsibility of the repo.
    /// </summary>
    public class MemoryConfigStore : IConfigStore
    {
        private readonly Mutex readWriteLock = new();

        // key = path
        private readonly ConcurrentDictionary<string, ConfigStoreValue> repo = new();

        public Task<ConfigStoreValue> GetValueAsync(string path)
        {
            path.EnsureValue(nameof(path));
            try
            {
                readWriteLock.WaitOne();
                return repo.TryGetValue(path, out var value)
                    ? Task.FromResult(value)
                    : Task.FromResult(new ConfigStoreValue(path, null, false));
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }

        public Task<ConfigStoreValue> SetValueAsync(SetConfigStoreValueRequest value)
        {
            value.EnsureValue(nameof(value));
            try
            {
                readWriteLock.WaitOne();
                repo[value.Path] = new ConfigStoreValue(value.Path, value.Value, true);
                return Task.FromResult(repo[value.Path]);
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }
    }
}