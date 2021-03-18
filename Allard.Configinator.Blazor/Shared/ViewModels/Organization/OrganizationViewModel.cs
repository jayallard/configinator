using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public record OrganizationViewModel
    {
        public string OrganizationId { get; set; }
        public List<RealmViewModel> Realms { get; init; }
        public List<Link> Links { get; set; }
        public RealmViewModel GetRealm(string realmId)
        {
            return Realms.Single(r => r.RealmId == realmId);
        }
    }
}