using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetConfigurationValueHandler : IRequestHandler<GetConfigurationValueCommand, ConfigurationValue>
    {
        private readonly Configinator configinator;

        public GetConfigurationValueHandler(Configinator configinator)
        {
            this.configinator = configinator;
        }

        public async Task<ConfigurationValue> Handle(GetConfigurationValueCommand request,
            CancellationToken cancellationToken)
        {
            // todo: convert to dto - it may be an exact copy, but still... don't
            // return the domain object
            var id = new ConfigurationId(request.Habitat, request.Realm, request.ConfigurationSection);
            return await configinator.Configuration.Get(id);
        }
    }
}