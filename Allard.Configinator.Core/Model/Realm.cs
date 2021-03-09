using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Events;
using Allard.Configinator.Core.Model.Validators;

namespace Allard.Configinator.Core.Model
{
    public class Realm
    {
        private readonly Dictionary<string, ConfigurationSection> configurationSections = new();
        private readonly Dictionary<string, Habitat> habitats = new();

        public Realm(OrganizationAggregate organization, RealmId realmId)
        {
            Organization = organization;
            RealmId = realmId;
        }

        public OrganizationAggregate Organization { get; }
        public RealmId RealmId { get; }
        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;
        public IReadOnlyCollection<ConfigurationSection> ConfigurationSections => configurationSections.Values;

        public Habitat GetHabitat(string habitatName)
        {
            if (habitats.TryGetValue(habitatName, out var habitat)) return habitat;

            throw new InvalidOperationException("Habitat doesn't exist: " + habitat);
        }

        public ConfigurationSection GetConfigurationSection(string configurationSectionName)
        {
            if (configurationSections.TryGetValue(configurationSectionName, out var cs)) return cs;

            throw new InvalidOperationException("Configuration section doesn't exist: " + cs);
        }

        /// <summary>
        ///     Used by the event handler.
        /// </summary>
        /// <param name="habitat"></param>
        internal void AddHabitat(Habitat habitat)
        {
            habitats.Add(habitat.HabitatId.Name, habitat);
        }

        /// <summary>
        ///     Used by the event handler.
        /// </summary>
        /// <param name="configurationSection"></param>
        internal void AddConfigurationSection(ConfigurationSection configurationSection)
        {
            configurationSections.Add(configurationSection.ConfigurationSectionId.Name, configurationSection);
        }

        public ConfigurationSection AddConfigurationSection(
            string configurationSectionName,
            string schemaTypeId,
            string path,
            string description)
        {
            return AddConfigurationSection(configurationSectionName, SchemaTypeId.Parse(schemaTypeId), path,
                description);
        }

        public ConfigurationSection AddConfigurationSection(
            string configurationSectionName,
            SchemaTypeId schemaTypeId,
            string path,
            string description)
        {
            path.EnsureValue(nameof(path));
            var configurationSectionId = ConfigurationSectionId.NewConfigurationSectionId(configurationSectionName);

            // make sure the configuration section doesn't already exist
            habitats.Keys.EnsureNameDoesntAlreadyExist(configurationSectionId);

            // lazy - throws an exception if type doesn't exist.
            Organization.GetSchemaType(schemaTypeId);

            // create and raise the event
            var evt = new AddedConfigurationSectionToRealmEvent(
                Organization.OrganizationId,
                RealmId,
                configurationSectionId,
                schemaTypeId,
                path,
                description);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedConfigurationSectionToRealmEvent, ConfigurationSection>(evt);
        }

        public Habitat AddHabitat(string habitName, params string[] baseHabitats)
        {
            return AddHabitat(habitName, baseHabitats.ToHashSet());
        }

        public Habitat AddHabitat(string habitatName, ISet<string> baseHabitats = null)
        {
            var habitatId
                = HabitatId.NewHabitatId(habitatName);
            baseHabitats = baseHabitats.ToNormalizedMemberNames(nameof(baseHabitats));

            // make sure habitat doesn't already exist.
            habitats.Keys.EnsureNameDoesntAlreadyExist(habitatId);

            // make sure the hierarchy is sound.
            // ie: no circular references, self references, invalid references.
            ValidateHabitatHierarchy(habitatName, baseHabitats);

            // get the ids of the base habitats
            var baseIds = habitats
                .Values
                .Where(h => baseHabitats.Contains(h.HabitatId.Name))
                .Select(h => h.HabitatId)
                .ToHashSet();

            // create and raise the event.
            var evt = new AddedHabitatToRealmEvent(Organization.OrganizationId, RealmId, habitatId, baseIds);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedHabitatToRealmEvent, Habitat>(evt);
        }

        private void ValidateHabitatHierarchy(string habitatName, IEnumerable<string> baseHabitats)
        {
            var toTest = new HierarchyElement(habitatName, baseHabitats.ToHashSet());
            var existingHabitats = habitats
                .Values
                .Select(h =>
                    new HierarchyElement(h.HabitatId.Name, h.Bases.Select(b => b.HabitatId.Name).ToHashSet()));
            HierarchyValidator.Validate(toTest, existingHabitats);
        }
    }
}