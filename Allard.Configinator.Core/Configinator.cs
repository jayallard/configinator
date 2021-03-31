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
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            var partialUpdate = !string.IsNullOrWhiteSpace(request.SettingsPath);

            var model = structureModelBuilder.ToStructureModel(cs);
            var habitatDoc = request.Value;
            if (partialUpdate)
            {
                // partial - expand the input doc to match the doc structure,
                // and add it to the merge list.
                var habitatJson = (await GetValueFromConfigstore(cs, habitat));
                var expandedJson = JsonUtility.Expand(request.SettingsPath, request.Value);
                var merged1 = (await DocMerger3.Merge(model, habitatJson, expandedJson));
                var merged1Json = merged1.ToJsonString("1");
                habitatDoc = JsonDocument.Parse(merged1Json);
            }

            var resolver = new ValueResolver(Organization, configStore);
            var validator = new DocValidator(Organization.SchemaTypes, habitat.HabitatId.Id);
            var errors = validator.Validate(cs.Properties.ToList(), habitatDoc.RootElement).ToList();
            var descendentHabitats = realm.Habitats.Where(h => h.BaseHabitat == habitat);
            var results = new List<HabitatValue>
            {
                new(habitat.HabitatId, errors, habitatDoc)
            };

            foreach (var d in descendentHabitats)
            {
                var childResults = await resolver.ApplyValue(d, cs, model, habitatDoc);
                results.AddRange(childResults);
            }

            return new SetValueResponse(request.ConfigurationId, errors);
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            var configsToGet = GetValueFromConfigstore(cs, habitat);
            return null;
            // var toMerge = (await configsToGet).ToList();
            // var model = structureModelBuilder.ToStructureModel(cs);
            // var merged = await DocMerger3.Merge(model, toMerge.Select(m => m.Item1));
            // var value = GetDeepValue(merged, request.ValuePath, habitat.HabitatId);
            // var anyExists = toMerge.Any(m => m.Item2.Exists);
            // return new GetValueResponse(request.ConfigurationId, anyExists, value, merged);
        }

        /// <summary>
        /// Drill into a config object to pull out a specific object or value.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="settingPath"></param>
        /// <param name="habitatId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static JsonDocument GetDeepValue(ObjectValue values, string settingPath, HabitatId habitatId)
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

        // TODO: this is duplicated in value resolver. fix.
        private async Task<JsonDocument> GetValueFromConfigstore(
            ConfigurationSection cs, Habitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = await configStore.GetValueAsync(path);
            return value.Exists
                ? value.Value
                : JsonDocument.Parse("{}");
        }
    }

    public record HabitatValue(HabitatId HabitatId, List<ValidationFailure> Errors, JsonDocument Value);
}