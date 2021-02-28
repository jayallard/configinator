using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetRealmHandler : IRequestHandler<GetRealmCommand, RealmViewModel>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;

        public GetRealmHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }

        public async Task<RealmViewModel> Handle(GetRealmCommand request, CancellationToken cancellationToken)
        {
            var model = (await configinator.Realms.ByName(request.Name))
                .ToRealmViewModel();
            
            // this sets everything recursively including top realm level.
            LinkHelper.AddLinksToRealm(linkHelper, model, true);
            
            // this overwrites realm level... sloppy. todo: would this all be better as extension methods?
            model
                .SetLinks(linkHelper
                .CreateBuilder()
                .AddRoot()
                .AddRealms()
                .AddRealm(request.Name, true)
                .Build()
                .ToList());
            return model;
        }
    }
}