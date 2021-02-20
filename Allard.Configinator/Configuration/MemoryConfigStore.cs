using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
{
    /// <summary>
    /// For demo/test purposes only.
    /// Uses a dictionary as a configuration store.
    ///
    /// The assumption is that any backing store will reject
    /// requests if the hash doesn't match. Thus, it really
    /// is the responsibility of the repo.
    /// </summary>
    public class MemoryConfigStore : IConfigStore
    {
        // key = path
        private readonly ConcurrentDictionary<string, ConfigurationSectionValue> repo = new();
        private readonly Mutex readWriteLock = new();

        public Task<ConfigurationSectionValue> GetConfiguration(Habitat habitat, ConfigurationSection section)
        {
            try
            {
                readWriteLock.WaitOne();
                var key = habitat.Name + "::" + section.Path;
                return repo.TryGetValue(key, out var value)
                    ? Task.FromResult(value)
                    : Task.FromResult<ConfigurationSectionValue>(null);
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }

        public async Task WriteConfiguration(ConfigurationSectionValue value)
        {
            try
            {
                readWriteLock.WaitOne();
                var existing = await GetConfiguration(value.Habitat, value.Section);
                if (existing != null && existing.ETag != value.ETag)
                {
                    throw new Exception("etag change");
                }

                // need a new etag if the object is new, or if the object content has changed.
                // todo: determine if this is the appropriate behavior. i think it is.
                // also, if content didn't change, then no need to update.
                var needNewEtag = existing == null || existing.Value != value.Value;
                var toWrite =
                    needNewEtag
                        ? value with {ETag = Guid.NewGuid().ToString()}
                        : existing;
                var key = value.Habitat.Name + "::" + value.Section.Path;
                repo[key] = toWrite;
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }
    }
}