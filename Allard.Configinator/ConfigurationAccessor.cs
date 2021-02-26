using System;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;

namespace Allard.Configinator
{
    public class ConfigurationAccessor
    {
        private readonly Func<ConfigurationId, Task<ConfigurationSectionValue>> getter;
        private readonly Func<ConfigurationSectionValue, Task> setter;

        public ConfigurationAccessor(
            Func<ConfigurationId, Task<ConfigurationSectionValue>> getter,
            Func<ConfigurationSectionValue, Task> setter
        )
        {
            this.getter = getter.EnsureValue(nameof(getter));
            this.setter = setter.EnsureValue(nameof(setter));
        }

        public async Task Set(ConfigurationSectionValue value)
        {
            await setter(value).ConfigureAwait(false);
        }

        public async Task<ConfigurationSectionValue> Get(ConfigurationId id)
        {
            return await getter(id).ConfigureAwait(false);
        }
    }
}