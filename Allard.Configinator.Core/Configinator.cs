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
    public class Configinator : IConfiginator
    {
        private readonly IConfigStore configStore;
        private readonly OrganizationAggregate org;

        public Configinator(OrganizationAggregate org, IConfigStore configStore)
        {
            this.org = org.EnsureValue(nameof(org));
            this.configStore = configStore.EnsureValue(nameof(configStore));
        }

        public OrganizationAggregate Organization => org;

        public async Task<SetConfigurationResponse> SetValueResolvedAsync(SetConfigurationRequest request)
        {
            
        }

        public async Task<SetConfigurationResponse> SetValueRawAsync(SetConfigurationRequest request)
        {
            var realm = org.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get all of the docs for the base habitats, if there are any.
            var toMerge = await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList());

            // add the current request to the doc list.
            var requestDoc = new JsonObjectNode("", request.Value.RootElement);
            var requestMerge = new DocumentToMerge(request.ConfigurationId.HabitatId, requestDoc);
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

                // save the value that was passed in. 
                var value = new SetConfigStoreValueRequest(path, request.Value);
                await configStore.SetValueAsync(value);
            }

            return new SetConfigurationResponse(request.ConfigurationId, errors);
        }

        public async Task<GetConfigurationResponse> GetValueResolvedAsync(GetConfigurationRequest request)
        {
            var realm = org.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            var toMerge = await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList().AddIfNotNull(habitat));
            var merged = (await DocMerger.Merge(toMerge)).ToList();

            // todo: doo much conversion
            var mergedJsonString = JsonDocument.Parse(merged.ToJsonString());
            return new GetConfigurationResponse(request.ConfigurationId, false, mergedJsonString, merged);
        }

        private ConfigurationSection GetConfigurationSection(ConfigurationId id)
        {
            var realm = org.GetRealmByName(id.RealmId);
            return realm.GetConfigurationSection(id.SectionId);
        }

        public async Task<GetConfigurationResponse> GetValueRawAsync(GetConfigurationRequest request)
        {
            var cs = GetConfigurationSection(request.ConfigurationId);
            var habitat = cs.Realm.GetHabitat(request.ConfigurationId.HabitatId);
            var path = cs.Path.Replace("{{habitat}}", habitat.HabitatId.Name);
            var value = await configStore.GetValueAsync(path);
            return new GetConfigurationResponse(request.ConfigurationId, value.Exists, value.Value, null);
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
                    if (ct.GetTask.Result.Value == null) return null;

                    return new DocumentToMerge(ct.Habitat.HabitatId.Name,
                        new JsonObjectNode(string.Empty, ct.GetTask.Result.Value.RootElement));
                })
                .Where(v => v != null)
                .ToList();
        }
    }
}