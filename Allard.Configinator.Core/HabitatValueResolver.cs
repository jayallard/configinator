using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public class HabitatValueResolver
    {
        private readonly JsonElement model;
        private readonly Func<IHabitat, Task<JsonDocument>> configStore;
        private readonly Tree<HabitatId, IHabitat> habitats;
        private readonly Dictionary<HabitatId, HabitatConfigurationVersioning> results = new();

        public HabitatValueResolver(
            JsonDocument model,
            Func<IHabitat, Task<JsonDocument>> configStore,
            HabitatId rootHabitatId,
            List<IHabitat> allHabitats)
        {
            this.configStore = configStore.EnsureValue(nameof(configStore));
            habitats = Configinator.GetHabitatTree(rootHabitatId, allHabitats);
            this.model = model.RootElement;
        }

        public async Task<List<HabitatConfigurationVersioning>> Resolve()
        {
            habitats.Root.Visit(async leaf =>
            {
                var value = (await configStore(habitats.Root.Value)).RootElement;
                leaf.Visit(leaf2 =>
                {
                    // the leaf id is the habitat id. get the habitat versioned json.
                    var versioned = GetOrCreateVersionedObject(leaf2.Value);
                    versioned.Object.AddVersion(leaf2.Id.Id, value);
                });
            });
            try
            {
                return results.Values.ToList();
            }
            finally
            {
                results.Clear();
            }
        }

        // private void SetValueFromHabitat(JsonElement parentValue, HabitatId parentId, Tree<HabitatId, IHabitat>.Leaf<HabitatId, IHabitat> leaf)
        // {
        //     var versioned = GetOrCreateVersionedObject(leaf.Id);
        //     versioned.Object.AddVersion(parentId.Id, parentValue);
        //     foreach (var child in leaf.Children)
        //     {
        //         SetValueFromHabitat(parentValue, parentId, child);
        //     }
        // }

        private HabitatConfigurationVersioning GetOrCreateVersionedObject(IHabitat habitat)
        {
            if (results.ContainsKey(habitat.HabitatId))
            {
                return results[habitat.HabitatId];
            }

            var result = new HabitatConfigurationVersioning(habitat.HabitatId, new JsonVersionedObject(model));
            results[habitat.HabitatId] = result;
            return result;
        }
    }

    public record HabitatConfigurationVersioning(HabitatId HabitatId, JsonVersionedObject Object);
}