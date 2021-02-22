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
        private readonly ConcurrentDictionary<string, ConfigurationSectionValue> repo = new();

        public Task<ConfigurationSectionValue> GetValue(Habitat habitat, ConfigurationSection section)
        {
            habitat = habitat ?? throw new ArgumentNullException(nameof(habitat));
            section = section ?? throw new ArgumentNullException(nameof(section));
            try
            {
                readWriteLock.WaitOne();
                var key = habitat.Name + "::" + section.Path;
                return repo.TryGetValue(key, out var value)
                    ? Task.FromResult(new ConfigurationSectionValue(value.Habitat, value.ConfigurationSection,
                        value.ETag, value.Value))
                    : Task.FromResult<ConfigurationSectionValue>(null);
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }

        public async Task SetValueAsync(ConfigurationSectionValue value)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            try
            {
                readWriteLock.WaitOne();
                var existing = await GetValue(value.Habitat, value.ConfigurationSection);
                if (existing != null && existing.ETag != value.ETag) throw new Exception("etag change");

                var etag =
                    existing == null || existing.Value != value.Value
                        ? Guid.NewGuid().ToString()
                        : existing.ETag;

                var key = value.Habitat.Name + "::" + value.ConfigurationSection.Path;
                repo[key] = new ConfigurationSectionValue(value.Habitat, value.ConfigurationSection, etag, value.Value);
            }
            finally
            {
                readWriteLock.ReleaseMutex();
            }
        }
    }
}