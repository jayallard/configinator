using System.Collections.Generic;

namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class HabitatViewModel
    {
        public string HabitatId { get; set; }
        public List<string> BaseHabitatIds { get; set; } = new();
    }
}