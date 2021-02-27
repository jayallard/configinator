using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Api.Commands.ViewModels;
using Allard.Configinator.Api.Controllers;
using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public class GetRealmsHandler : IRequestHandler<GetRealmsCommand, IEnumerable<RealmViewModel>>
    {
        private readonly Configinator configinator;
        private readonly LinkHelper linkHelper;

        public GetRealmsHandler(Configinator configinator, LinkHelper linkHelper)
        {
            this.configinator = configinator;
            this.linkHelper = linkHelper;
        }

        public async Task<IEnumerable<RealmViewModel>> Handle(GetRealmsCommand request,
            CancellationToken cancellationToken)
        {
            return (await configinator.Realms.All())
                .Select(r => ViewModelExtensionMethods.ToRealmViewModel(r, linkHelper));
        }
    }
}