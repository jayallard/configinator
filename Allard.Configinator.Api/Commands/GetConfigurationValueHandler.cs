using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetConfigurationValueHandler : IRequestHandler<GetConfigurationValueCommand, ConfigurationValueResponse>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;

        public GetConfigurationValueHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }
        
        public async Task<ConfigurationValueResponse> Handle(GetConfigurationValueCommand request, CancellationToken cancellationToken)
        {
            var id = new ConfigurationId(request.Habitat, request.Realm, request.ConfigurationSection);
            var value = await configinator.Configuration.Get(id);
            return new ConfigurationValueResponse
            {
                Value = value.Value
            };
        }
    }

    public class ConfigurationValueResponse
    {
        public string Value { get; set; }
    }
}