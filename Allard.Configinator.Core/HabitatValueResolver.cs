using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.DocumentMerger;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core
{
    public class HabitatValueResolver
    {
        private readonly JsonElement model;
        private readonly Func<HabitatId, JsonDocument> configStore;
        private readonly Tree<HabitatId, IHabitat> habitats;
        private readonly Dictionary<HabitatId, HabitatConfigurationVersioning> results = new();

        public HabitatValueResolver(
            JsonDocument model,
            Func<HabitatId, JsonDocument> configStore,
            Tree<HabitatId, IHabitat> habitats)
        {
            this.configStore = configStore;
            this.habitats = habitats;
            this.model = model.RootElement;
        }

        public List<HabitatConfigurationVersioning> Resolve()
        {
            results.Clear();
            var value = configStore(habitats.Root.Id);
            SetValueFromHabitat(value.RootElement, habitats.Root.Id, habitats.Root);
            return results.Values.ToList();
        }

        private void SetValueFromHabitat(JsonElement parentValue, HabitatId parentId, Tree<HabitatId, IHabitat>.Leaf<HabitatId, IHabitat> leaf)
        {
            var versioned = GetOrCreateVersionedObject(leaf.Id);
            versioned.Object.AddVersion(parentId.Id, parentValue);
            foreach (var child in leaf.Children)
            {
                SetValueFromHabitat(parentValue, parentId, child);
            }
        }

        private HabitatConfigurationVersioning GetOrCreateVersionedObject(HabitatId habitatId)
        {
            if (results.ContainsKey(habitatId))
            {
                return results[habitatId];
            }

            var result = new HabitatConfigurationVersioning(habitatId, new JsonVersionedObject(model));
            results[habitatId] = result;
            return result;
        }
    }

    public record HabitatConfigurationVersioning(HabitatId HabitatId, JsonVersionedObject Object);
}