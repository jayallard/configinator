using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    internal class ValueResolver
    {
        private readonly OrganizationAggregate org;
        private readonly IConfigStore configStore;

        public ValueResolver(OrganizationAggregate org, IConfigStore configStore)
        {
            this.org = org;
            this.configStore = configStore;
        }

        private async Task<JsonDocument> GetValueFromConfigstore(
            ConfigurationSection cs, IHabitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = await configStore.GetValueAsync(path);
            return value.Exists
                ? value.Value
                : JsonDocument.Parse("{}");
        }

        public async Task<List<HabitatValue>> ApplyValue(
            IHabitat habitat,
            ConfigurationSection cs,
            JsonDocument model,
            JsonDocument value)
        {
            var habitatValue = await GetValueFromConfigstore(cs, habitat);
            var merged = await DocMerger3.Merge(model, value, habitatValue);
            var mergedJson = JsonDocument.Parse(merged.ToJsonString(habitat.HabitatId.Id));
            var descendentHabitats = cs.Realm.Habitats.Where(h => h.BaseHabitat == habitat);

            var validator = new DocValidator(org.SchemaTypes, habitat.HabitatId.Id);
            var validationFailures = validator.Validate(cs.Properties.ToList(), mergedJson.RootElement).ToList();

            var results = new List<HabitatValue>
            {
                new(habitat.HabitatId, validationFailures, mergedJson)
            };

            foreach (var d in descendentHabitats)
            {
                var childResults = await ApplyValue(d, cs, model, value);
                results.AddRange(childResults);
            }

            return results;
        }
    }
}