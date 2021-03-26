using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Xsl;
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
            var configDocs = (await GetDocsFromConfigStore(cs, habitat.Bases.ToList())).ToList();
            var toMerge = configDocs.Select(d => d.Item1).ToList();

            // add the current request to the doc list.
            var requestMerge = new DocumentToMerge(request.ConfigurationId.HabitatId, request.Value);
            toMerge.Add(requestMerge);

            // merge
            var model = structureModelBuilder.ToStructureModel(cs);
            var merged = await DocMerger3.Merge(model, toMerge);
            // TODO: change to single object.
            var mergedJson = merged.ToJsonString();

            var mergedDoc = JsonDocument.Parse(mergedJson);

            // validate
            var validator = new DocValidator(Organization.SchemaTypes);
            var errors = validator.Validate(cs.Properties.ToList(), mergedDoc.RootElement).ToList();

            // if no errors, save
            if (errors.Count > 0) return new SetConfigurationResponse(request.ConfigurationId, errors);

            // save
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);

            // if it's resolved format, then reduce the input value down to just the values
            // that changed in the last query.
            // if there's only one doc, then nothing to reduce, do
            // skip it
            var toSave = request.Value;
            if (request.Format == ValueFormat.Resolved)
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
                _ => throw new ArgumentOutOfRangeException(nameof(request))
            };
        }

        private static JsonDocument ReduceToRawJson(ObjectValue o)
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
            return new ObjectValue(o.ObjectPath, o.Name, childProperties.AsReadOnly(), childObjects.AsReadOnly());
        }

        private async Task<GetConfigurationResponse> GetValueRawAsync(GetValueRequest request)
        {
            if (request.ValuePath != null)
            {
                throw new NotImplementedException("TODO: setting path not supported yet");
            }

            var cs = GetConfigurationSection(request.ConfigurationId);
            var habitat = cs.Realm.GetHabitat(request.ConfigurationId.HabitatId);
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = await configStore.GetValueAsync(path);
            return new GetConfigurationResponse(request.ConfigurationId, value.Exists, value.Value, null);
        }

        private async Task<GetConfigurationResponse> GetValueResolvedAsync(GetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            var toMerge = (await GetDocsFromConfigStore(cs, habitat.Bases.ToList().AddIfNotNull(habitat)))
                .ToList();
            var model = structureModelBuilder.ToStructureModel(cs);
            var merged = await DocMerger3.Merge(model, toMerge.Select(m => m.Item1));
            var value = GetValue(merged, request.ValuePath);
            //var mergedJsonDoc = JsonDocument.Parse(merged.ToJsonString());
            var anyExists = toMerge.Any(m => m.Item2.Exists);
            return new GetConfigurationResponse(request.ConfigurationId, anyExists, value, merged);
        }

        /// <summary>
        /// Drill into a config object to pull out a specific object or value.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="settingPath"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static JsonDocument GetValue(ObjectValue values, string settingPath)
        {
            if (string.IsNullOrWhiteSpace(settingPath))
            {
                return JsonDocument.Parse(values.ToJsonString());
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
                return property.Value == null ? null : JsonDocument.Parse(property.Value);
            }
            
            // if the path resolves to a node, then return the node as json.
            var node = currentObject.Objects.SingleOrDefault(p => p.Name == parts[^1]);
            if (node == null)
            {
                throw new InvalidOperationException("Invalid setting name. Failed Path=" + settingPath);
            }

            return JsonDocument.Parse(node.ToJsonString());
        }

        private ConfigurationSection GetConfigurationSection(ConfigurationId id)
        {
            return Organization
                .GetRealmByName(id.RealmId)
                .GetConfigurationSection(id.SectionId);
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