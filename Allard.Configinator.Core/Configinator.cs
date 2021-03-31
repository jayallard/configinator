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


            var habitatTree = GetHabitatTree(habitat.HabitatId, realm.Habitats.ToList());
            // TODO: partial
            foreach (var h in habitatTree.Root.Children)
            {
                
            }
            
            // var chains = GetHabitatDescendantChains(habitat.HabitatId, realm.Habitats.ToList());
            // var model = structureModelBuilder.ToStructureModel(cs);
            // foreach (var chain in chains)
            // {
            //     var versioned = new JsonVersionedObject(model.RootElement);
            //     foreach (var h in chain)
            //     {
            //         var habitatValue = await GetValueFromConfigstore(cs, h);
            //         versioned.AddVersion(h.HabitatId.Id, habitatValue.RootElement);
            //     }
            // }
            
            var partialUpdate = !string.IsNullOrWhiteSpace(request.SettingsPath);

            //var model = structureModelBuilder.ToStructureModel(cs);
            //var habitatDoc = request.Value;
            if (partialUpdate)
            {
                // partial - expand the input doc to match the doc structure,
                // and add it to the merge list.
                // var habitatJson = (await GetValueFromConfigstore(cs, habitat));
                // var expandedJson = JsonUtility.Expand(request.SettingsPath, request.Value);
                // var merged1 = (await DocMerger3.Merge(model, habitatJson, expandedJson));
                // var merged1Json = merged1.ToJsonString("1");
                // habitatDoc = JsonDocument.Parse(merged1Json);
            }

            // var resolver = new ValueResolver(Organization, configStore);
            // var validator = new DocValidator(Organization.SchemaTypes, habitat.HabitatId.Id);
            // var errors = validator.Validate(cs.Properties.ToList(), habitatDoc.RootElement).ToList();
            // var descendentHabitats = realm.Habitats.Where(h => h.BaseHabitat == habitat);
            // var results = new List<HabitatValue>
            // {
            //     new(habitat.HabitatId, errors, habitatDoc)
            // };
            //
            // foreach (var d in descendentHabitats)
            // {
            //     var childResults = await resolver.ApplyValue(d, cs, model, habitatDoc);
            //     results.AddRange(childResults);
            // }

            return new SetValueResponse(request.ConfigurationId, null /*errors*/);
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // get the bases and the specific value, then merge.
            //var configsToGet = GetValueFromConfigstore(cs, habitat);
            var habitats = GetHabitatTree(habitat.HabitatId, realm.Habitats.ToList());
            
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
            ConfigurationSection cs, IHabitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = await configStore.GetValueAsync(path);
            return value.Exists
                ? value.Value
                : JsonDocument.Parse("{}");
        }

        public static Tree<HabitatId, IHabitat> GetHabitatTree(HabitatId rootHabitatId, List<IHabitat> allHabitats)
        {
            var rootHabitat = allHabitats.Single(h => h.HabitatId == rootHabitatId);
            var tree = new Tree<HabitatId, IHabitat>(rootHabitatId, rootHabitat);
            AddChildren(tree, allHabitats, rootHabitatId);
            return tree;
        }

        private static void AddChildren(Tree<HabitatId, IHabitat> tree, List<IHabitat> allHabitats, HabitatId currentId)
        {
            var children = allHabitats.Where(h => h.BaseHabitat?.HabitatId == currentId).ToList();
            if (children.Count == 0)
            {
                // done
                return;
            }

            foreach (var child in children)
            {
                tree.Add(currentId, child.HabitatId, child);
                AddChildren(tree, allHabitats, child.HabitatId);
            }
        }

        /// <summary>
        /// Gets all of the descendent dependency chains for a habitat.
        /// </summary>
        /// <param name="targetHabitatId"></param>
        /// <param name="allHabitats"></param>
        /// <returns></returns>
        public static List<List<IHabitat>> GetHabitatDescendantChains(HabitatId targetHabitatId,
            List<IHabitat> allHabitats)
        {
            // get all the habitats that are parents.
            var bases = allHabitats
                .Where(h => h.BaseHabitat != null)
                .Select(h => h.BaseHabitat.HabitatId)
                .ToHashSet();

            // find the habitats that don't have any children
            // these are the bottom-level habitats.
            var bottomHabitats = allHabitats
                .Where(h => !bases.Contains(h.HabitatId))
                .ToList();

            var chains = new List<List<IHabitat>>();
            foreach (var bottom in bottomHabitats)
            {
                var current = bottom;
                var chain = new List<IHabitat>();
                while (current != null)
                {
                    chain.Add(current);
                    if (current.HabitatId == targetHabitatId)
                    {
                        break;
                    }

                    current = current.BaseHabitat;
                }

                if (current != null)
                {
                    chains.Add(chain);
                }


                // put the base class as the top
                chain.Reverse();
            }

            return chains;
        }
    }

    public record HabitatConfigurationValue(IHabitat Habitat, JsonVersionedObject);

    public record HabitatValue(HabitatId HabitatId, List<ValidationFailure> Errors, JsonDocument Value);
}