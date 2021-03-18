using System.Collections.Generic;

namespace Allard.Configinator.Api.Commands.ViewModels
{
    public record SchemaTypesViewModel(IEnumerable<SchemaTypeViewModel> SchemaTypes, IEnumerable<Link> Links);
}