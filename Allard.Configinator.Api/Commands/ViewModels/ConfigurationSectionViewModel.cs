using System.Collections.Generic;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public class ConfigurationSectionViewModel
    {
        public OrganizationId OrganizationId { get; set; }
        public RealmId RealmId { get; set; }
        public ConfigurationSectionId ConfigurationSectionId { get; set; }
        public string Path { get; set; }
        public string SchemaTypeId { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}