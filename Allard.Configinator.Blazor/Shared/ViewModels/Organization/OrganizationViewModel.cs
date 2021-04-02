using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class OrganizationViewModel
    {
        [StringLength(20, MinimumLength = 5)]
        [RegularExpression(Constants.NameRegex,
            ErrorMessage = "The OrganizationId must be lower case letters and dashes.")]
        public string OrganizationId { get; set; }

        public List<RealmViewModel> Realms { get; set; }
        public List<Link> Links { get; set; }
        public List<SchemaTypeViewModel> SchemaTypes { get; set; }

        public RealmViewModel GetRealm(string realmId)
        {
            return Realms.Single(r => r.RealmId == realmId);
        }

        public SchemaTypeViewModel GetSchemaType(string schemaTypeId)
        {
            return SchemaTypes.Single(t => t.SchemaTypeId == schemaTypeId);
        }
    }
}