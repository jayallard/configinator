using System.Threading.Tasks;

namespace Allard.Configinator.Configuration
{
    public interface IConfigurationRepository
    {
        Task<ConfigurationSectionValue> GetConfiguration(ConfigurationSection section);
        Task WriteConfiguration(ConfigurationSectionValue configurationSection);
    }
}