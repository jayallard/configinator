using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public class HabitatValueResolver
    {
        private readonly IHabitat baseHabitat;
        private readonly Func<IHabitat, Task<ObjectDto>> configStore;
        private readonly Dictionary<IHabitat, VersionTracker> habitatTrackers = new();

        public HabitatValueResolver(
            ObjectDto objectModel,
            Func<IHabitat, Task<ObjectDto>> configStore,
            IHabitat baseHabitat)
        {
            objectModel.EnsureValue(nameof(objectModel));
            this.configStore = configStore.EnsureValue(nameof(configStore));
            this.baseHabitat = baseHabitat.EnsureValue(nameof(baseHabitat));
            
            // create a new tracker for each habitat
            Visit(baseHabitat, h =>
            {
                var tracker = new VersionTracker(objectModel, h.HabitatId.Id);
                habitatTrackers.Add(h, tracker);
            });
        }

        public IEnumerable<VersionTracker> VersionedHabitats => habitatTrackers.Values;

        public async Task LoadExistingValues()
        {
            await Task.Run(() =>
            {
                Visit(baseHabitat, async h =>
                {
                    var value = await configStore(h);
                    var tracker = habitatTrackers[h];

                    if (h.BaseHabitat != null)
                    {
                        var baseTracker = habitatTrackers[h.BaseHabitat];
                        tracker.AddVersion(h.BaseHabitat.HabitatId.Id, baseTracker.Versions.Last().ToObjectDto());
                    }

                    tracker.AddVersion(h.HabitatId.Id, value);
                });
            });
        }

        public void OverwriteValue(IHabitat habitat, ObjectDto newValue, string path = null)
        {
            var tracker = habitatTrackers[habitat];
            tracker.UpdateVersion(habitat.HabitatId.Id, newValue, path);
            Visit(habitat, h =>
            {
                if (habitat == h) return;

                var childTracker = habitatTrackers[h];
                childTracker.UpdateVersion(h.BaseHabitat.HabitatId.Id, tracker.Versions.Last().ToObjectDto());
                CopyDown(childTracker);
            });
        }

        private static void CopyDown(VersionTracker tracker)
        {
            if (tracker.Versions.Count == 1)
                // nothing to copy
                return;

            CopyDown(tracker.Versions.Last());
        }

        private static void CopyDown(VersionedObject obj)
        {
            foreach (var property in obj.Properties)
                // if the base and child used to have the same value,
                // then they should continue to have the same value.
                // IE: base=x, child=x.
                //     the base changed to y, so change the child to y
                if (property.Value == null || string.Equals(property.Value, property.PreviousVersion.OriginalValue))
                    property.SetValue(property.PreviousVersion.Value);
            
            foreach (var o in obj.Objects)
            {
                CopyDown(o);
            }
        }
        
        private static void Visit(IHabitat habitat, Action<IHabitat> visitor)
        {
            visitor(habitat);
            foreach (var child in habitat.Children) Visit(child, visitor);
        }
    }
}