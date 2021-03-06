using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public class Configinator
    {
        private readonly OrganizationAggregate org;
        private readonly IConfigStore configStore;

        public Configinator(OrganizationAggregate org, IConfigStore configStore)
        {
            this.org = org.EnsureValue(nameof(org));
            this.configStore = configStore.EnsureValue(nameof(configStore));
        }

        private async Task<string> GetPathAsync(ConfigurationId id)
        {
            // var ns = await Realms.ByName(id.Realm).ConfigureAwait(false);
            // var cs = ns.GetConfigurationSection(id.ConfigurationSection);
            // var habitat = await habitatService.GetHabitatAsync(id.Habitat).ConfigureAwait(false);
            // return cs.Path.Replace("{{habitat}}", habitat.Name);
            return null;
        }

        private async Task SetValueAsync()
        {
        }

        private async Task<ConfigurationValue> GetValueAsync(ConfigurationId id)
        {
            var realm = org.GetRealm(id.Realm);
            var habitat = realm.GetHabitat(id.Habitat);
            var cs = realm.GetConfigurationSection(id.ConfigurationSection);

            // todo: deal with org later

            // get all values.
            var configTasks = habitat
                .Bases
                .ToList()
                .AddNonNullItem(habitat)
                .Select(h =>
                {
                    var path = cs.Path.Replace("{{habitat}}", habitat.HabitatId.Name);
                    return new
                    {
                        GetTask = configStore.GetValueAsync(path),
                        Habitat = h
                    };
                }).ToList();

            await Task.WhenAll(configTasks.Select(c => c.GetTask));
            var docs = configTasks
                .Select(ct =>
                {
                    var json = JsonDocument.Parse(ct.GetTask.Result.Value).RootElement;
                    return new DocumentToMerge(ct.Habitat.HabitatId.Name, new JsonObjectNode(string.Empty, json));
                });

            // merge the docs
            var merged = await DocMerger.Merge(docs);
            var mergedJsonString = merged.ToJsonString();
            return new ConfigurationValue(id, "todo", mergedJsonString);
        }
    }
}