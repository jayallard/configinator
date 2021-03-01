using System;
using System.Threading.Tasks;

namespace Allard.Configinator
{
    public class ConfigurationAccessor
    {
        private readonly Func<ConfigurationId, Task<ConfigurationValue>> getter;
        private readonly Func<ConfigurationValueSetter, Task> setter;

        public ConfigurationAccessor(
            Func<ConfigurationId, Task<ConfigurationValue>> getter,
            Func<ConfigurationValueSetter, Task> setter
        )
        {
            this.getter = getter.EnsureValue(nameof(getter));
            this.setter = setter.EnsureValue(nameof(setter));
        }

        public async Task Set(ConfigurationValueSetter value)
        {
            await setter(value).ConfigureAwait(false);
        }

        public async Task<ConfigurationValue> Get(ConfigurationId id)
        {
            return await getter(id).ConfigureAwait(false);
        }
    }
}