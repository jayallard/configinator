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
        /*
         * Stores one tracker per habitat.
         * IE Given this habitat tree:
         *              H1
         *          H2A     H2B
         *                      H3C
         *
         * There are 4 trackers.
         * Each tracker has up to 2 versions: itself, and its base.
         * H1 doesn't have a base, so it only has one version.
         * H2A contains versions: H1, H2A.
         * H3C contains versions: H3C, H2B.
         *
         * To start, a tracker is created for each habitat.
         * LoadExistingValues adds the versions to the tracker.
         * - the base version is copied from the base's tracker
         * - the current version is retrieved from the config store.
         *
         * Now, everything is loaded.
         * When a habitat value is updated via OverWrite value:
         * - the habitat is updated with the new value
         * - each habitat below it is updated
         *    - the version that represents the base is overwritten with the new version
         *    - the base version is resolved against the current version.
         *
         * The merge strategy is simple:
         *  - when a base value changes
         *      - if the sub habitat has the same value as the original base value (pre-change),
         *        or if the value for the sub habitat is null,
         *              then the sub value is assigned the value of the base.
         *  
         * IE: if the base value changes from a to b:
         *     if the sub habitat value is a, it also changes to b
         *     if the sub habitat value is x, it doesn't change
         *     if the sub habitat value is null, it is set to b
         *
         * This cascades down the tree where each value is considered against
         * the new value of its base.
         */


        private readonly IHabitat habitat;
        private readonly Func<IHabitat, Task<ObjectDto>> configStore;
        private readonly Dictionary<HabitatId, VersionTracker> habitatTrackers = new();

        public HabitatValueResolver(
            ObjectDto objectModel,
            Func<IHabitat, Task<ObjectDto>> configStore,
            IHabitat habitat)
        {
            objectModel.EnsureValue(nameof(objectModel));
            this.configStore = configStore.EnsureValue(nameof(configStore));
            this.habitat = habitat.EnsureValue(nameof(habitat));

            // var current = habitat.BaseHabitat;
            // while (current != null)
            // {
            //     var tracker = new VersionTracker(objectModel, current.HabitatId.Id);
            //     habitatTrackers.Add(current.HabitatId, tracker);
            //     current = current.BaseHabitat;
            // }
            
            // create a new tracker for each child habitat
            Visit(habitat, h =>
            {
                var tracker = new VersionTracker(objectModel, h.HabitatId.Id);
                habitatTrackers.Add(h.HabitatId, tracker);
            });
        }

        public IEnumerable<VersionTracker> VersionedHabitats => habitatTrackers.Values;

        public async Task LoadExistingValues()
        {
            await Task.Run(() =>
            {
                // Iterate the tree of habitats. Get the configuration value for each.
                // set 2 versions of the object in the tracker:
                //  1 - the value of the base habitat (if there is one)
                //  2 - the value of the current habitat as provided by the config store.
                Visit(habitat, async h =>
                {
                    if (h == habitat)
                    {
                        return;
                    }
                    
                    // get the value for the habitat
                    var value = await configStore(h);

                    // get the tracker for the habitat
                    var tracker = habitatTrackers[h.HabitatId];
                    if (h.BaseHabitat != null && habitatTrackers.ContainsKey(h.BaseHabitat.HabitatId))
                    {
                        // if the habitat has a base, get its tracker.
                        var baseTracker = habitatTrackers[h.BaseHabitat.HabitatId];

                        // copy the values from the base's tracker to this tracker.
                        if (baseTracker.Versions.Any())
                        {
                            tracker.AddVersion(h.BaseHabitat.HabitatId.Id, baseTracker.Versions.Last().ToObjectDto());
                        }
                    }

                    // add this habitat's value to the tracker.
                    // now the tracker has 2 values: the copy of it's base's value, and its own value.
                    tracker.AddVersion(h.HabitatId.Id, value);
                });
            });
        }

        /// <summary>
        /// Replace the value of a habitat.
        /// </summary>
        /// <param name="habitat">The habitat with the updated value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="path">The path of the value. Used for partial updates. If null or empty, then the entire object is updated.</param>
        public void OverwriteValue(IHabitat habitat, ObjectDto newValue, string path = null)
        {
            // update the tracker with the new value for the habitat.
            var tracker = habitatTrackers[habitat.HabitatId];
            tracker.UpdateVersion(habitat.HabitatId.Id, newValue, path);
            Process(habitat);
            
            // not using VISIT in this case, because its more efficient not too.
            // with VISIT, the BASE would reload for each child, and convert to DTO.
            // by using this loop instead, we can load it and convert it once,
            // and reuse for all children. slightly more efficient.
            void Process(IHabitat h)
            {
                var baseTracker = habitatTrackers[h.HabitatId];
                var baseDto = baseTracker.Versions.Last().ToObjectDto();
                foreach (var child in h.Children)
                {
                    // get the trackers for the habitat to update.
                    var childTracker = habitatTrackers[child.HabitatId];

                    // update the habitat tracker with the values from the base.
                    childTracker.UpdateVersion(child.BaseHabitat.HabitatId.Id, baseDto);

                    // copy the values from the base to the habitat
                    ResolveHabitatFromBase(childTracker.Versions.Last());

                    // cascade
                    Process(child);
                }
            }
        }

        /// <summary>
        /// Copy the values from the BASE version (version 0)
        /// to the HABITAT version (version 1)
        /// </summary>
        /// <param name="obj"></param>
        private static void ResolveHabitatFromBase(VersionedObject obj)
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
                ResolveHabitatFromBase(o);
            }
        }

        /// <summary>
        /// Visit the habitat, and all of its descendants.
        /// </summary>
        /// <param name="habitat"></param>
        /// <param name="visitor"></param>
        private static void Visit(IHabitat habitat, Action<IHabitat> visitor)
        {
            visitor(habitat);
            foreach (var child in habitat.Children) Visit(child, visitor);
        }
    }
}