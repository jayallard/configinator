using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class SetConfigurationValueHandler : IRequestHandler<SetConfigurationValueCommand, Blah>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;

        public SetConfigurationValueHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }

        public async Task<Blah> Handle(SetConfigurationValueCommand request, CancellationToken cancellationToken)
        {
            // todo: wrong types... in progress.
            var id = new ConfigurationId(request.Habitat, request.Realm, request.ConfigurationSection);
            var setter = new ConfigurationValueSetter(id, request.PreviousEtag, request.Value);
            await configinator.Configuration.Set(setter);
            return new Blah();
        }
    }

    public class Blah
    {
    }
}