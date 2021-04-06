using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly ObjectDto objectModel;

        public HabitatValueResolver(
            ObjectDto objectModel,
            Func<IHabitat, Task<ObjectDto>> configStore,
            IHabitat baseHabitat)
        {
            this.objectModel = objectModel.EnsureValue(nameof(objectModel));
            this.configStore = configStore.EnsureValue(nameof(configStore));
            this.baseHabitat = baseHabitat.EnsureValue(nameof(baseHabitat));
            Initialize();
        }

        public IEnumerable<VersionedObject> Habitats => habitatTrackers
            .Values
            .Select(t => t.Versions.Last());

        public IEnumerable<VersionedObject> ChangedHabitats => habitatTrackers
            .Values
            .Where(t => t.Versions.Last().IsChanged)
            .Select(t => t.Versions.Last());


        /// <summary>
        ///     Crawls the habitat tree. Creates a tracker for each habitat.
        /// </summary>
        private void Initialize()
        {
            Visit(baseHabitat, _ =>
            {
                var tracker = new VersionTracker(objectModel);
                habitatTrackers.Add(baseHabitat, tracker);
            });
        }

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
                var childTracker = habitatTrackers[h];
                childTracker.UpdateVersion(habitat.HabitatId.Id, tracker.Versions.Last().ToObjectDto());
                CopyDown(childTracker);
            });
        }

        private void CopyDown(VersionTracker tracker)
        {
            if (tracker.Versions.Count == 1)
                // nothing to copy
                return;

            CopyDown(tracker.Versions.Last().Objects);
        }

        private void CopyDown(IEnumerable<VersionedObject> objects)
        {
            foreach (var obj in objects)
            {
                foreach (var property in obj.Properties)
                    // if the base and child used to have the same value,
                    // then they should continue to have the same value.
                    // IE: base=x, child=x.
                    //     the base changed to y, so change the child to y
                    if (property.Value == null || string.Equals(property.Value, property.OriginalValue))
                        property.SetValue(property.PreviousVersion.OriginalValue);

                CopyDown(obj.Objects);
            }
        }

        private void Visit(IHabitat habitat, Action<IHabitat> visitor)
        {
            visitor(habitat);
            foreach (var child in habitat.Children) Visit(child, visitor);
        }
    }

    public record HabitatConfigurationVersioning(HabitatId HabitatId, VersionedObject Object);
}