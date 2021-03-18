using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public record SchemaTypesViewModel(IEnumerable<SchemaTypeViewModel> SchemaTypes, IEnumerable<Link> Links);
}