using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public class Configinator : IConfiginator
    {
        private readonly IConfigStore configStore;

        public Configinator(OrganizationAggregate org, IConfigStore configStore)
        {
            Organization = org.EnsureValue(nameof(org));
            this.configStore = configStore.EnsureValue(nameof(configStore));
        }

        public OrganizationAggregate Organization { get; }

        public async Task<SetValueResponse> SetValueAsync(SetValueRequest request)
        {
            // super messy... once logic is finalized, cleanup and refactor
            
            
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            async Task<JsonDocument> ConfigResolver(IHabitat h)
            {
                return await GetValueFromConfigstore(cs, h);
            }

            // TODO: if a single habitat, simplify.. 
            var model = StructureBuilder.ToStructure(cs);
            var resolver = new HabitatValueResolver(model, ConfigResolver, habitat);
            await resolver.LoadExistingValues();
            var newValue = request.Value.ToObjectDto();
            // todo: convert versioned object instead... one less conversion
            resolver.OverwriteValue(habitat, newValue);

            var state = resolver
                .Habitats
                .Select(h => new State
                {
                    Habitat = realm.GetHabitat(h.VersionName),
                    IsChanged = h.IsChanged,
                    Object = h.IsChanged ? h.ToObjectDto() : null,
                    IsSaved = false
                })
                .ToList();

            if (state.All(s => !s.IsChanged))
            {
                var habitats = state
                    .Select(h =>
                        new SetValueResponseHabitat(h.IsChanged, h.IsSaved, h.Habitat.HabitatId.Id,
                            h.Failures))
                    .ToList();
                return new SetValueResponse(habitats);
            }

            // validate
            foreach (var s in state.Where(s => s.IsChanged))
            {
                var failures = new ConfigurationValidator(cs, Organization.SchemaTypes)
                    .Validate(s.Habitat.HabitatId, s.Object).ToList();
                s.Failures.AddRange(failures);
            }

            if (state.Any(s => !s.CanSave))
            {
                var habitats = state
                    .Select(h =>
                        new SetValueResponseHabitat(h.IsChanged, h.IsSaved, h.Habitat.HabitatId.Id,
                            h.Failures))
                    .ToList();
                return new SetValueResponse(habitats);
            }

            // save
            foreach (var s in state.Where(s => s.CanSave))
            {
                // todo: change config store to take a list in case it can do them all in a transaction
                var json = s.Object.ToJson();
                var path = OrganizationAggregate.GetConfigurationPath(cs, s.Habitat);
                var r = new SetConfigStoreValueRequest(path, json);
                await configStore.SetValueAsync(r);
                s.IsSaved = true;
            }

            var h = state
                .Select(h =>
                    new SetValueResponseHabitat(h.IsChanged, h.IsSaved, h.Habitat.HabitatId.Id,
                        h.Failures))
                .ToList();
            return new SetValueResponse(h);
        }

        private class State
        {
            public ObjectDto Object { get; set; }
            public IHabitat Habitat { get; set; }
            public List<ValidationFailure> Failures { get; } = new();
            public bool IsChanged { get; set; }
            public bool IsSaved { get; set; }
            public bool CanSave => IsChanged && Failures.Count == 0;
        }

        private ConfigurationId CreateConfigurationId(ConfigurationSection configurationSection, IHabitat habitat)
        {
            return new(
                Organization.OrganizationId.Id,
                configurationSection.Realm.RealmId.Id,
                configurationSection.SectionId.Id,
                habitat.HabitatId.Id);
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            //var configsToGet = GetValueFromConfigstore(cs, habitat);
            //var habitats = GetHabitatTree(habitat.HabitatId, realm.Habitats.ToList());

            return null;
            // var toMerge = (await configsToGet).ToList();
            // var model = structureModelBuilder.ToStructureModel(cs);
            // var merged = await DocMerger3.Merge(model, toMerge.Select(m => m.Item1));
            // var value = GetDeepValue(merged, request.ValuePath, habitat.HabitatId);
            // var anyExists = toMerge.Any(m => m.Item2.Exists);
            // return new GetValueResponse(request.ConfigurationId, anyExists, value, merged);
        }

        /// <summary>
        ///     Drill into a config object to pull out a specific object or value.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="settingPath"></param>
        /// <param name="habitatId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /*
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
        */
        private async Task<JsonDocument> GetValueFromConfigstore(
            ConfigurationSection cs, IHabitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = await configStore.GetValueAsync(path);
            return value.Exists
                ? value.Value
                : JsonDocument.Parse("{}");
        }

        // public static Tree<HabitatId, IHabitat> GetHabitatTree(HabitatId rootHabitatId, List<IHabitat> allHabitats)
        // {
        //     var rootHabitat = allHabitats.Single(h => h.HabitatId == rootHabitatId);
        //     var tree = new Tree<HabitatId, IHabitat>(rootHabitatId, rootHabitat);
        //     AddChildren(tree, allHabitats, rootHabitatId);
        //     return tree;
        // }

        // private static void AddChildren(Tree<HabitatId, IHabitat> tree, List<IHabitat> allHabitats, HabitatId currentId)
        // {
        //     var children = allHabitats.Where(h => h.BaseHabitat?.HabitatId == currentId).ToList();
        //     if (children.Count == 0)
        //         // done
        //         return;
        //
        //     foreach (var child in children)
        //     {
        //         tree.Add(currentId, child.HabitatId, child);
        //         AddChildren(tree, allHabitats, child.HabitatId);
        //     }
        // }

        /// <summary>
        ///     Gets all of the descendent dependency chains for a habitat.
        /// </summary>
        /// <param name="targetHabitatId"></param>
        /// <param name="allHabitats"></param>
        /// <returns></returns>
        // public static List<List<IHabitat>> GetHabitatDescendantChains(HabitatId targetHabitatId,
        //     List<IHabitat> allHabitats)
        // {
        //     // get all the habitats that are parents.
        //     var bases = allHabitats
        //         .Where(h => h.BaseHabitat != null)
        //         .Select(h => h.BaseHabitat.HabitatId)
        //         .ToHashSet();
        //
        //     // find the habitats that don't have any children
        //     // these are the bottom-level habitats.
        //     var bottomHabitats = allHabitats
        //         .Where(h => !bases.Contains(h.HabitatId))
        //         .ToList();
        //
        //     var chains = new List<List<IHabitat>>();
        //     foreach (var bottom in bottomHabitats)
        //     {
        //         var current = bottom;
        //         var chain = new List<IHabitat>();
        //         while (current != null)
        //         {
        //             chain.Add(current);
        //             if (current.HabitatId == targetHabitatId) break;
        //
        //             current = current.BaseHabitat;
        //         }
        //
        //         if (current != null) chains.Add(chain);
        //
        //
        //         // put the base class as the top
        //         chain.Reverse();
        //     }
        //
        //     return chains;
        // }
    }

    //public record HabitatConfigurationValue(IHabitat Habitat, JsonVersionedObject Value);

    //public record HabitatValue(HabitatId HabitatId, List<ValidationFailure> Errors, JsonDocument Value);
}