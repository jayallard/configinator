using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public class HabitatValueResolver
    {
        private readonly ObjectDto objectModel;
        private readonly Func<IHabitat, Task<JsonDocument>> configStore;
        private readonly IHabitat baseHabitat;
        private readonly Dictionary<IHabitat, VersionTracker> habitatTrackers = new();

        public HabitatValueResolver(
            ObjectDto objectModel,
            Func<IHabitat, Task<JsonDocument>> configStore,
            IHabitat baseHabitat)
        {
            this.objectModel = objectModel.EnsureValue(nameof(objectModel));
            this.configStore = configStore.EnsureValue(nameof(configStore));
            this.baseHabitat = baseHabitat.EnsureValue(nameof(baseHabitat));
            Initialize();
        }

        public IEnumerable<VersionedObject> ChangedHabitats => habitatTrackers
            .Values
            .Where(t => t.Objects.Last().IsChanged)
            .Select(t => t.Objects.Last());


        /// <summary>
        /// Crawls the habitat tree. Creates a tracker for each habitat.
        /// </summary>
        private void Initialize()
        {
            Visit(baseHabitat, h =>
            {
                var tracker = new VersionTracker(objectModel);
                habitatTrackers.Add(baseHabitat, tracker);
            });
        }

        public async Task LoadExistingValues()
        {
            Visit(baseHabitat, async h =>
            {
                var valueJson = await configStore(h);
                var value = ToObjectDto(valueJson.RootElement);
                var tracker = habitatTrackers[h];

                if (h.BaseHabitat != null)
                {
                    var baseTracker = habitatTrackers[h.BaseHabitat];
                    tracker.AddVersion(h.BaseHabitat.HabitatId.Id, baseTracker.Objects.Last().ToDto());
                }
                
                tracker.AddVersion(h.HabitatId.Id, value);
            });
        }

        public void OverwriteValue(IHabitat habitat, ObjectDto newValue)
        {
            var tracker = habitatTrackers[habitat];
            tracker.Update(habitat.HabitatId.Id, newValue);
            Visit(habitat, h =>
            {
                var childTracker = habitatTrackers[h];
                childTracker.Update(habitat.HabitatId.Id, tracker.Objects.Last().ToDto());
                CopyDown(childTracker);
            });
        }

        private void CopyDown(VersionTracker tracker)
        {
            
        }
        
        private static ObjectDto ToObjectDto(JsonElement configValue)
        {
            return new();
        }

        private void Visit(IHabitat habitat, Action<IHabitat> visitor)
        {
            visitor(habitat);
            foreach (var child in habitat.Children)
            {
                Visit(child, visitor);
            }
        }

    }

    public record HabitatConfigurationVersioning(HabitatId HabitatId, VersionedObject Object);
}