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
    public class MemoryConfigurationRepository : IConfigurationRepository
    {
        private readonly ConcurrentDictionary<ConfigurationSection, ConfigurationSectionValue> repo = new();
        private readonly Mutex readWriteLock = new();

        public Task<ConfigurationSectionValue> GetConfiguration(ConfigurationSection id)
        {
            try
            {
                readWriteLock.WaitOne();
                return repo.TryGetValue(id, out var value)
                    ? Task.FromResult(value)
                    : Task.FromResult<ConfigurationSectionValue>(null);
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }

        public async Task WriteConfiguration(ConfigurationSectionValue configurationSection)
        {
            try
            {
                readWriteLock.WaitOne();
                var existing = await GetConfiguration(configurationSection.Id);
                if (existing != null && existing.ETag != configurationSection.ETag)
                {
                    throw new Exception("etag change");
                }
                
                // need a new etag if the object is new, or if the object content has changed.
                // todo: determine if this is the appropriate behavior. i think it is.
                // also, if content didn't change, then no need to update.
                var needNewEtag = existing == null || existing.Value != configurationSection.Value;
                var toWrite =
                    needNewEtag
                        ? configurationSection with {ETag = Guid.NewGuid().ToString()}
                        : existing;
                repo[toWrite.Id] = toWrite;
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }
    }
}