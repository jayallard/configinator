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

        public async Task<SetConfigurationResponse> SetValueAsync(SetConfigurationRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get all of the docs for the base habitats, if there are any.
            var toMerge = await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList());

            // add the current request to the doc list.
            var requestMerge = new DocumentToMerge(request.ConfigurationId.HabitatId, request.Value);
            toMerge.Add(requestMerge);

            // merge
            var merged = (await DocMerger.Merge(toMerge)).ToList();
            var mergedJson = merged.ToJsonString();

            // todo: get rid of ??
            var mergedDoc = JsonDocument.Parse(mergedJson ?? "{}");

            // validate
            var errors = new DocValidator(Organization.SchemaTypes)
                .Validate(cs.SchemaType.SchemaTypeId, mergedDoc)
                .ToList();

            // if no errors, save
            if (errors.Count == 0)
            {
                // save
                // todo: normalize this
                var path = cs.Path.Replace("{{habitat}}", habitat.HabitatId.Id);

                // if it's resolved format, then reduce the input value down to just the values
                // that changed in the last query.
                // if there's only one doc, then nothing to reduce, do
                // skip it
                var toSave = request.Value;
                if (request.Format == ValueFormat.Resolved && merged.First().Property.Layers.Count > 1)
                    toSave = ReduceToRawJson(merged);

                // save the value that was passed in. 
                var value = new SetConfigStoreValueRequest(path, toSave);
                await configStore.SetValueAsync(value);
            }

            return new SetConfigurationResponse(request.ConfigurationId, errors);
        }

        public async Task<GetConfigurationResponse> GetValueAsync(GetValueRequest request)
        {
            return request.Format switch
            {
                ValueFormat.Raw => await GetValueRawAsync(request),
                ValueFormat.Resolved => await GetValueResolvedAsync(request),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static JsonDocument ReduceToRawJson(List<MergedProperty> properties)
        {
            var reduced = properties
                .Where(p =>
                {
                    // only keep properties that were somehow changed 
                    // in the last layer.
                    var last = p.Property.Layers.Last();
                    return last.Transition == Transition.Set || last.Transition == Transition.Delete;
                });
            return JsonDocument.Parse(reduced.ToJsonString());
        }

        private async Task<GetConfigurationResponse> GetValueRawAsync(GetValueRequest request)
        {
            var cs = GetConfigurationSection(request.ConfigurationId);
            var habitat = cs.Realm.GetHabitat(request.ConfigurationId.HabitatId);
            var path = cs.Path.Replace("{{habitat}}", habitat.HabitatId.Id);
            var value = await configStore.GetValueAsync(path);
            return new GetConfigurationResponse(request.ConfigurationId, value.Exists, value.Value, null);
        }

        private async Task<GetConfigurationResponse> GetValueResolvedAsync(GetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            var toMerge = await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList().AddIfNotNull(habitat));
            var model = structureModelBuilder.ToStructureModel(cs);
            var merged = (await DocMerger2.Merge(model, toMerge)).ToList();

            // todo: too much conversion
            var mergedJsonString = JsonDocument.Parse(merged.ToJsonString());
            return new GetConfigurationResponse(request.ConfigurationId, merged.Count > 0, mergedJsonString, merged);
        }

        private ConfigurationSection GetConfigurationSection(ConfigurationId id)
        {
            return Organization
                .GetRealmByName(id.RealmId)
                .GetConfigurationSection(id.SectionId);
        }

        private async Task<List<DocumentToMerge>> GetDocsFromConfigStore(string path,
            IEnumerable<Habitat> habitats)
        {
            // get all values.
            var configTasks = habitats
                .Select(h =>
                {
                    var resolvedPath = path.Replace("{{habitat}}", h.HabitatId.Id);
                    return new
                    {
                        GetTask = configStore.GetValueAsync(resolvedPath),
                        Habitat = h
                    };
                }).ToList();

            await Task.WhenAll(configTasks.Select(c => c.GetTask));
            return configTasks
                .Select(ct => ct.GetTask.Result.Value == null
                    ? new DocumentToMerge(ct.Habitat.HabitatId.Id, JsonDocument.Parse("{}"))
                    : new DocumentToMerge(ct.Habitat.HabitatId.Id, ct.GetTask.Result.Value))
                .ToList();
        }
    }
}