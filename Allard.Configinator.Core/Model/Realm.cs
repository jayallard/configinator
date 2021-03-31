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
            string description)
        {
            return AddConfigurationSection(new SectionId(sectionId), properties,
                description);
        }

        public ConfigurationSection AddConfigurationSection(
            SectionId sectionId,
            IReadOnlyCollection<SchemaTypeProperty> properties,
            string description)
        {
            // make sure the configuration section doesn't already exist
            habitats.Keys.EnsureIdDoesntExist(sectionId);

            if (!properties.Any())
            {
                throw new InvalidOperationException("At least one property is required.");
            }

            Organization.EnsureValidSchemaTypes(properties.Select(p => p.SchemaTypeId));
            // todo: duplicate names

            // create and raise the event
            var evt = new AddedConfigurationSectionToRealmEvent(
                Organization.OrganizationId,
                RealmId,
                sectionId,
                properties,
                description);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedConfigurationSectionToRealmEvent, ConfigurationSection>(evt);
        }

        public Habitat AddHabitat(string habitatId, string baseHabitatId)
        {
            return AddHabitat(new HabitatId(habitatId), baseHabitatId);
        }

        public Habitat AddHabitat(HabitatId habitatId, string baseHabitatId = null)
        {
            // make sure habitat doesn't already exist.
            habitats.Keys.EnsureIdDoesntExist(habitatId);

            // make sure the hierarchy is sound.
            // ie: no circular references, self references, invalid references.
            // ValidateHabitatHierarchy(habitatId, baseHabitats);
            //
            // // get the ids of the base habitats
            // var baseIds = habitats
            //     .Values
            //     .Where(h => baseHabitats.Contains(h.HabitatId))
            //     .Select(h => h.HabitatId)
            //     .ToHashSet();

            // create and raise the event.
            var evt = new AddedHabitatToRealmEvent(Organization.OrganizationId, RealmId, habitatId, null);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedHabitatToRealmEvent, Habitat>(evt);
        }

        private void ValidateHabitatHierarchy(HabitatId habitatId, IEnumerable<HabitatId> baseHabitats)
        {
            // var toTest = new HierarchyElement(habitatId.Id, baseHabitats.Select(b => b.Id).ToHashSet());
            // var existingHabitats = habitats
            //     .Values
            //     .Select(h =>
            //         new HierarchyElement(h.HabitatId.Id, new List<Habitat> {habitatId}.AsReadOnly()).ToHashSet()));
            // HierarchyValidator.Validate(toTest, existingHabitats);
        }
    }
}