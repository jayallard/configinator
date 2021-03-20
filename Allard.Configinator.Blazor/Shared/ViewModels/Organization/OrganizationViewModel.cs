using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class OrganizationViewModel
    {
        public string OrganizationId { get; set; }
        public List<RealmViewModel> Realms { get; set; }
        public List<Link> Links { get; set; }
        public List<SchemaTypeViewModel> SchemaTypes { get; set; }
        public RealmViewModel GetRealm(string realmId)
        {
            return Realms.Single(r => r.RealmId == realmId);
        }
    }
}