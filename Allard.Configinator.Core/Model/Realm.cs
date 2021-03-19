using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model.Validators;

namespace Allard.Configinator.Core.Model
{
    public class Realm
    {
        private readonly Dictionary<SectionId, ConfigurationSection> configurationSections = new();
        private readonly Dictionary<HabitatId, Habitat> habitats = new();

        public Realm(OrganizationAggregate organization, RealmId realmId)
        {
            Organization = organization;
            RealmId = realmId;
        }

        public OrganizationAggregate Organization { get; }
        public RealmId RealmId { get; }
        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;
        public IReadOnlyCollection<ConfigurationSection> ConfigurationSections => configurationSections.Values;

        public Habitat GetHabitat(string habitatId)
        {
            var id = new HabitatId(habitatId);
            if (habitats.TryGetValue(id, out var habitat)) return habitat;

            throw new InvalidOperationException("Habitat doesn't exist. HabitatId= " + habitatId);
        }

        public ConfigurationSection GetConfigurationSection(string sectionId)
        {
            var id = new SectionId(sectionId);
            if (configurationSections.TryGetValue(id, out var cs)) return cs;

            throw new InvalidOperationException("Configuration section doesn't exist: " + cs);
        }

        /// <summary>
        ///     Used by the event handler.
        /// </summary>
        /// <param name="habitat"></param>
        internal void AddHabitat(Habitat habitat)
        {
            habitats.Add(habitat.HabitatId, habitat);
        }

        /// <summary>
        ///     Used by the event handler.
        /// </summary>
        /// <param name="configurationSection"></param>
        internal void AddConfigurationSection(ConfigurationSection configurationSection)
        {
            configurationSections.Add(configurationSection.SectionId, configurationSection);
        }

        public ConfigurationSection AddConfigurationSection(
            string sectionId,
            IReadOnlyCollection<SchemaTypeProperty> properties,
            string path,
            string description)
        {
            return AddConfigurationSection(new SectionId(sectionId), properties, path,
                description);
        }

        public ConfigurationSection AddConfigurationSection(
            SectionId sectionId,
            IReadOnlyCollection<SchemaTypeProperty> properties,
            string path,
            string description)
        {
            path.EnsureValue(nameof(path));

            // make sure the configuration section doesn't already exist
            habitats.Keys.EnsureIdDoesntExist(sectionId);

            // lazy - throws an exception if type doesn't exist.
            var _ = properties.Select(p => Organization.GetSchemaType(p.SchemaTypeId)).ToList();

            // create and raise the event
            var evt = new AddedConfigurationSectionToRealmEvent(
                Organization.OrganizationId,
                RealmId,
                sectionId,
                properties,
                path,
                description);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedConfigurationSectionToRealmEvent, ConfigurationSection>(evt);
        }

        public Habitat AddHabitat(string habitatId, params string[] baseHabitats)
        {
            return AddHabitat(new HabitatId(habitatId), baseHabitats.Select(h => new HabitatId(h)).ToHashSet());
        }

        public Habitat AddHabitat(HabitatId habitatId, ISet<HabitatId> baseHabitats = null)
        {
            // make sure habitat doesn't already exist.
            habitats.Keys.EnsureIdDoesntExist(habitatId);

            // make sure the hierarchy is sound.
            // ie: no circular references, self references, invalid references.
            ValidateHabitatHierarchy(habitatId, baseHabitats);

            // get the ids of the base habitats
            var baseIds = habitats
                .Values
                .Where(h => baseHabitats.Contains(h.HabitatId))
                .Select(h => h.HabitatId)
                .ToHashSet();

            // create and raise the event.
            var evt = new AddedHabitatToRealmEvent(Organization.OrganizationId, RealmId, habitatId, baseIds);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedHabitatToRealmEvent, Habitat>(evt);
        }

        private void ValidateHabitatHierarchy(HabitatId habitatId, IEnumerable<HabitatId> baseHabitats)
        {
            var toTest = new HierarchyElement(habitatId.Id, baseHabitats.Select(b => b.Id).ToHashSet());
            var existingHabitats = habitats
                .Values
                .Select(h =>
                    new HierarchyElement(h.HabitatId.Id, h.Bases.Select(b => b.HabitatId.Id).ToHashSet()));
            HierarchyValidator.Validate(toTest, existingHabitats);
        }
    }
}