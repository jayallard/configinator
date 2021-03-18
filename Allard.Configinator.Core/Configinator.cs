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
            var configDocs = (await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList())).ToList();
            var toMerge = configDocs.Select(d => d.Item1).ToList();

            // add the current request to the doc list.
            var requestMerge = new DocumentToMerge(request.ConfigurationId.HabitatId, request.Value);
            toMerge.Add(requestMerge);

            // merge
            var model = structureModelBuilder.ToStructureModel(cs);
            var merged = (await DocMerger3.Merge(model, toMerge));
            // TODO: change to single object.
            var mergedJson = merged.ToJsonString();

            // todo: get rid of ??
            var mergedDoc = JsonDocument.Parse(mergedJson ?? "{}");

            // validate
            var errors = new DocValidator(Organization.SchemaTypes)
                .Validate(cs.SchemaType.SchemaTypeId, mergedDoc)
                .ToList();

            // if no errors, save
            if (errors.Count > 0) return new SetConfigurationResponse(request.ConfigurationId, errors);

            // save
            // todo: normalize this
            var path = cs.Path.Replace("{{habitat}}", habitat.HabitatId.Id);

            // if it's resolved format, then reduce the input value down to just the values
            // that changed in the last query.
            // if there's only one doc, then nothing to reduce, do
            // skip it
            var toSave = request.Value;
            if (request.Format == ValueFormat.Resolved) // && merged.First().Property.Layers.Count > 1)
                toSave = ReduceToRawJson(merged);

            // save the value that was passed in. 
            var value = new SetConfigStoreValueRequest(path, toSave);
            await configStore.SetValueAsync(value);

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

        public static JsonDocument ReduceToRawJson(ObjectValue o)
        {
            var reduced = ReduceToChanges(o);
            return JsonDocument.Parse(reduced.ToJsonString());
        }

        private static ObjectValue ReduceToChanges(ObjectValue o)
        {
            var childObjects = o
                .Objects
                .Select(ReduceToChanges)
                .ToList();

            var childProperties = o
                .Properties
                .Select(p =>
                {
                    var lastLayer = p.Layers.Last();
                    var changed = lastLayer.Transition == Transition.Set || lastLayer.Transition == Transition.Delete;
                    return changed ? p : null;
                })
                .Where(p => p != null)
                .ToList();
            return new ObjectValue(o.Path, o.Name, childProperties.AsReadOnly(), childObjects.AsReadOnly());
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
            var toMerge = (await GetDocsFromConfigStore(cs.Path, habitat.Bases.ToList().AddIfNotNull(habitat)))
                .ToList();
            var model = structureModelBuilder.ToStructureModel(cs);
            var merged = await DocMerger3.Merge(model, toMerge.Select(m => m.Item1));

            // todo: too much conversion
            var mergedJsonDoc = JsonDocument.Parse(merged.ToJsonString());
            var anyExists = toMerge.Any(m => m.Item2.Exists);
            return new GetConfigurationResponse(request.ConfigurationId, anyExists, mergedJsonDoc, merged);
        }

        private ConfigurationSection GetConfigurationSection(ConfigurationId id)
        {
            return Organization
                .GetRealmByName(id.RealmId)
                .GetConfigurationSection(id.SectionId);
        }

        private async Task<IEnumerable<(DocumentToMerge, ConfigStoreValue)>> GetDocsFromConfigStore(string path,
            IEnumerable<Habitat> habitats)
        {
            // get all values.
            var results = habitats
                .Select(async h =>
                {
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