using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Mvc.Views.Configinator;
using Microsoft.AspNetCore.Mvc;

namespace Allard.Configinator.Mvc.Controllers
{
    public class ConfiginatorController : Controller
    {
        private readonly IConfiginatorService configinatorService;

        public ConfiginatorController(IConfiginatorService configinatorService)
        {
            this.configinatorService = configinatorService;
        }

        // GET
        [Route("/configinator/{organizationId}/{realmId}/{sectionId}/{habitatId}")]
        public async Task<IActionResult> Index(string organizationId, string realmId, string sectionId,
            string habitatId)
        {
            var configinator = await configinatorService.GetConfiginatorByIdAsync(organizationId);
            var id = new ConfigurationId(organizationId, realmId, sectionId, habitatId);
            var resolved = await configinator.GetValueAsync(new GetValueRequest(id, ValueFormat.Resolved));
            var properties = resolved
                .PropertyDetail
                .Select(p =>
                {
                    var layers = p.Property.Layers.Select(l => new ExplainedPropertyLayer(
                        l.LayerName,
                        l.Transition.ToString(),
                        l.Value));

                    return new ExplainedProperty(
                        p.Path,
                        p.Property.Name,
                        p.Property.Value,
                        layers.Last().Transition,
                        layers
                            .ToList());
                })
                .ToList();
            return View(new ExplainedViewModel(properties));
        }
    }
}