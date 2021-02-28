using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http;

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
            var id = new ConfigurationId(request.Habitat, request.Realm, request.ConfigurationSection);
            var value = new ConfigurationSectionValue(
                id,
                request.PreviousEtag,
                request.Value?.ToString());

            await configinator.Configuration.Set(value);
            return new Blah();
        }
    }

    public class Blah
    {
    }
}