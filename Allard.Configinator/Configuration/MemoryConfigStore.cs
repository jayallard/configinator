using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
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
                    ? Task.FromResult(new ConfigStoreValue(path, value.ETag, value.Value))
                    : Task.FromResult(new ConfigStoreValue(path, null, null));
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }

        public async Task SetValueAsync(ConfigStoreValue value)
        {
            value.EnsureValue(nameof(value));
            try
            {
                readWriteLock.WaitOne();
                var existing = await GetValueAsync(value.Path).ConfigureAwait(false);
                if (existing.Value == null)
                {
                    // insert
                    repo[value.Path] = value with {ETag = Guid.NewGuid().ToString()};
                    return;
                }

                if (value.Value == existing.Value)
                    // no change. nothing to do.
                    return;

                // update
                if (value.ETag == null) throw new Exception("etag required");

                if (value.ETag != existing.ETag)
                    throw new Exception("Invalid etag - the value may have changed since the lst get.");

                repo[value.Path] = value with {ETag = Guid.NewGuid().ToString()};
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }
    }
}