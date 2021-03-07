using System.Collections.Generic;
using System.IO;
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

        private async Task<List<DocumentToMerge>> GetDocsFromConfigStore(string path,
            List<Habitat> habitats)
        {
            // get all values.
            var configTasks = habitats
                .Select(h =>
                {
                    var resolvedPath = path.Replace("{{habitat}}", h.HabitatId.Name);
                    return new
                    {
                        GetTask = configStore.GetValueAsync(resolvedPath),
                        Habitat = h
                    };
                }).ToList();

            await Task.WhenAll(configTasks.Select(c => c.GetTask));
            return configTasks
                .Select(ct =>
                {
                    if (ct.GetTask.Result.Value == null)
                    {
                        return null;
                    }

                    var json = JsonDocument.Parse(ct.GetTask.Result.Value).RootElement;
                    return new DocumentToMerge(ct.Habitat.HabitatId.Name, new JsonObjectNode(string.Empty, json));
                })
                .Where(v => v != null)
                .ToList();
        }

        public async Task<SetConfigurationResponse> SetValueAsync(SetConfigurationRequest request)
        {
            var realm = org.GetRealm(request.ConfigurationId.Realm);
            var habitat = realm.GetHabitat(request.ConfigurationId.Habitat);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.ConfigurationSection);

            // get all of the docs for the base habitats, if there are any.
            // doesn't include the value of the habit itself, because
            // we're validating the new value, not the existing.
            var toMerge = await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList());

            // add the current request to the doc list.
            var requestDoc = new JsonObjectNode("", JsonDocument.Parse(request.Value).RootElement);
            var requestMerge = new DocumentToMerge(request.ConfigurationId.Habitat, requestDoc);
            toMerge.Add(requestMerge);

            // merge
            var merged = await DocMerger.Merge(toMerge);
            var mergedJson = merged.ToJsonString();
            var mergedDoc = JsonDocument.Parse(mergedJson ?? "{}");

            // validate
            var errors = new DocValidator(org.SchemaTypes)
                .Validate(cs.SchemaTypeId, new JsonObjectNode("", mergedDoc.RootElement))
                .ToList();
            
            // if no errors, save
            if (errors.Count == 0)
            {
                // save
                // todo: normalize this
                var path = cs.Path.Replace("{{habitat}}", habitat.HabitatId.Name);
                var value = new ConfigStoreValue(path, "TODO", request.Value);
                await configStore.SetValueAsync(value);
            }

            return new SetConfigurationResponse(request.ConfigurationId, errors, null);
        }

        public async Task<GetConfigurationResponse> GetValueAsync(GetConfigurationRequest request)
        {
            var realm = org.GetRealm(request.ConfigurationId.Realm);
            var habitat = realm.GetHabitat(request.ConfigurationId.Habitat);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.ConfigurationSection);

            var toMerge = await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList().AddIfNotNull(habitat));
            var merged = (await DocMerger.Merge(toMerge)).ToList();
            var mergedJsonString = merged.ToJsonString();
            return new GetConfigurationResponse(request.ConfigurationId, "TODO", mergedJsonString, merged);
        }
    }
}