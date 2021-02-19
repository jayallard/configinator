using System;
using Allard.Configinator.Configuration;
using Allard.Configinator.Schema;

namespace Allard.Configinator
{
    public class ConfiginatorService
    {
        private readonly SchemaParser parser;
        private readonly IConfigurationRepository configRepository;

        public ConfiginatorService(SchemaParser parser, IConfigurationRepository configRepository)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        }
    }
}