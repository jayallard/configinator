using System;
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
        private readonly JsonStructureModelBuilder structureModelBuilder;

        public Configinator(OrganizationAggregate org, IConfigStore configStore)
        {
            Organization = org.EnsureValue(nameof(org));
            this.configStore = configStore.EnsureValue(nameof(configStore));
            structureModelBuilder = new JsonStructureModelBuilder(org.SchemaTypes);
        }

        public OrganizationAggregate Organization { get; }

        public async Task<SetValueResponse> SetValueAsync(SetValueRequest request)
        {
            // if it's a partial update: then get the values for all of the base habitats
            // and the habitat that is being updated.
            // then, merge that with the input doc, which is the partial udpate doc.

            // if it's not a partial update, then it's a full doc update.
            // so, get the values for the base habitats only, then merge
            // it with the input doc.

            // PARTIAL: merge  BASE 1 >> BASE 2 >> HABITAT >> INPUT   :   WRITE TO HABITAT
            // FULL:    merge  BASE 1 >> BASE 2 >> INPUT              :   WRITE TO HABITAT

            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            var partialUpdate = !string.IsNullOrWhiteSpace(request.SettingsPath);

            // get all of the docs for the base habitats, if there are any.
            var habitatsToGet = habitat.Bases.ToList();
            var configDocs = (await GetDocsFromConfigStore(cs, habitatsToGet)).ToList();
            var toMerge = configDocs.Select(d => d.Item1).ToList();

            var model = structureModelBuilder.ToStructureModel(cs);
            var habitatDoc = request.Value;
            if (partialUpdate)
            {
                // partial - expand the input doc to match the doc structure,
                // and add it to the merge list.
                var habitatJson = (await GetDocsFromConfigStore(cs, new[] {habitat})).Single();
                var expandedJson = JsonUtility.Expand(request.SettingsPath, request.Value);
                var merged1 = (await DocMerger3.Merge(model, habitatJson.Item1.Document, expandedJson));
                var merged1Json = merged1.ToJsonString("1");
                habitatDoc = JsonDocument.Parse(merged1Json);
            }

            // add the input doc to the merge list.
            var requestMerge = new DocumentToMerge(request.ConfigurationId.HabitatId, habitatDoc);
            toMerge.Add(requestMerge);

            // merge
            var merged = await DocMerger3.Merge(model, toMerge);
            var mergedJson = merged.ToJsonString(habitat.HabitatId.Id);
            var mergedDoc = JsonDocument.Parse(mergedJson);

            // validate
            var validator = new DocValidator(Organization.SchemaTypes);
            var errors = validator.Validate(cs.Properties.ToList(), mergedDoc.RootElement).ToList();

            // if no errors, save
            if (errors.Count > 0) return new SetValueResponse(request.ConfigurationId, errors);

            // save
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = new SetConfigStoreValueRequest(path, mergedDoc);
            await configStore.SetValueAsync(value);
            return new SetValueResponse(request.ConfigurationId, errors);
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request) 
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            var configsToGet = GetDocsFromConfigStore(cs, habitat.Bases.ToList().AddIfNotNull(habitat));
            var toMerge = (await configsToGet).ToList();
            var model = structureModelBuilder.ToStructureModel(cs);
            var merged = await DocMerger3.Merge(model, toMerge.Select(m => m.Item1));
            var value = GetValue(merged, request.ValuePath, habitat.HabitatId);
            var anyExists = toMerge.Any(m => m.Item2.Exists);
            return new GetValueResponse(request.ConfigurationId, anyExists, value, merged);
        }

        /// <summary>
        /// Drill into a config object to pull out a specific object or value.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="settingPath"></param>
        /// <param name="habitatId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static JsonDocument GetValue(ObjectValue values, string settingPath, HabitatId habitatId)
        {
            if (string.IsNullOrWhiteSpace(settingPath))
            {
                return JsonDocument.Parse(values.ToJsonString(habitatId.Id));
            }

            // all parts, except the last, are object references.
            var parts = settingPath.Split("/");
            var currentObject = values;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                var next = currentObject.Objects.SingleOrDefault(o => o.Name == parts[i]);
                if (next == null)
                {
                    var failedPath = string.Join("/", parts[..i]);
                    throw new InvalidOperationException("Invalid setting name. Failed Path=" + failedPath);
                }

                currentObject = next;
            }

            // if the path resolves to a property, then return the property value.
            var property = currentObject.Properties.SingleOrDefault(p => p.Name == parts[^1]);
            if (property != null)
            {
                // todo: harden
                return property.Value == null ? null : JsonDocument.Parse("\"" + property.Value + "\"");
            }

            // if the path resolves to a node, then return the node as json.
            var node = currentObject.Objects.SingleOrDefault(p => p.Name == parts[^1]);
            if (node == null)
            {
                throw new InvalidOperationException("Invalid setting name. Failed Path=" + settingPath);
            }

            return JsonDocument.Parse(node.ToJsonString(habitatId.Id));
        }

        private async Task<IEnumerable<(DocumentToMerge, ConfigStoreValue)>> GetDocsFromConfigStore(
            ConfigurationSection cs, IEnumerable<Habitat> habitats)
        {
            // get all values.
            var results = habitats
                .Select(async h =>
                {
                    var path = OrganizationAggregate.GetConfigurationPath(cs, h);
                    var resolvedPath = path.Replace("{{habitat}}", h.HabitatId.Id);
                    var value = await configStore.GetValueAsync(resolvedPath);
                    var v = value.Exists
                        ? value.Value
                        : JsonDocument.Parse("{}");
                    return (new DocumentToMerge(h.HabitatId.Id, v), value);
                });
            return await Task.WhenAll(results);
        }
    }
}