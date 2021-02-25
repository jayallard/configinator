using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Schema;

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
        private readonly ConcurrentDictionary<string, ConfigurationValue> repo = new();

        public Task<ConfigurationValue> GetValueAsync(string path)
        {
            path.EnsureValue(nameof(path));
            try
            {
                readWriteLock.WaitOne();
                return repo.TryGetValue(path, out var value)
                    ? Task.FromResult(new ConfigurationValue(path, value.ETag, value.Value))
                    : Task.FromResult(new ConfigurationValue(path, null, null));
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }

        public async Task SetValueAsync(ConfigurationValue value)
        {
            value.EnsureValue(nameof(value));
            try
            {
                readWriteLock.WaitOne();
                var existing = await GetValueAsync(value.Path);
                if (existing.ETag != null && existing.ETag != value.ETag) throw new Exception("etag change");

                var etag =
                    existing.ETag == null || existing.Value != value.Value
                        ? Guid.NewGuid().ToString()
                        : existing.ETag;

                repo[value.Path] = value with {ETag = etag};
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }
    }
}