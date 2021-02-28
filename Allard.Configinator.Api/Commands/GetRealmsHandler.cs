using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetRealmsHandler : IRequestHandler<GetRealmsCommand, RealmsViewModel>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;

        public GetRealmsHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }

        public async Task<RealmsViewModel> Handle(
            GetRealmsCommand request,
            CancellationToken cancellationToken)
        {
            var model = new RealmsViewModel
            {
                Realms = (await configinator.Realms.All())
                    .Select(r => r.ToRealmViewModel())
                    .ToList(),
                Links = linkHelper
                    .CreateBuilder()
                    .AddRoot()
                    .AddRealms(true)
                    .Build()
                    .ToList()
            };

            // set the links
            foreach (var realm in model.Realms)
            {
                LinkHelper.AddLinksToRealm(linkHelper, realm, false);
            }

            return model;
        }
    }
}