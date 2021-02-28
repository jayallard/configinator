using System.Collections.Generic;
using Allard.Configinator.Api.Commands.ViewModels;

namespace Allard.Configinator.Api.Controllers.ViewModels
{
    public class RootViewModel
    {
        public IEnumerable<Link> Links { get; set; }
    }
}