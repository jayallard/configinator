using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.DocumentValidator;
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

        private void Clear()
        {
            results.Clear();
        }

        public IEnumerable<HabitatConfigurationVersioning> Habitats => results.Values;

        public IEnumerable<HabitatConfigurationVersioning> ChangedHabitats =>
            results
                .Values
                .Where(h => h.Object.IsChanged);

        public async Task LoadExistingValues()
        {
            Clear();
            await Task.Run(() =>
            {
                // outer loop - load the config value for each node
                habitats.Root.Visit(async leaf =>
                {
                    // inner loop - apply the config to each child leaf
                    var value = (await configStore(habitats.Root.Value)).RootElement;
                    leaf.Visit(leaf2 =>
                    {
                        // the leaf id is the habitat id. get the habitat versioned json.
                        var versioned = GetOrCreateVersionedObject(leaf2.Value);
                        versioned.Object.AddVersion(leaf2.Id.Id, value);
                    });
                });
            });
        }

        public void OverwriteValue(HabitatId habitatId, JsonElement value)
        {
            results[habitatId].Object.UpdateVersion(habitatId.Id, value);
        }
        
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