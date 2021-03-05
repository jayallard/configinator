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

        public Realm(RealmId realmId, OrganizationAggregate organization)
        {
            Organization = organization;
            RealmId = realmId;
        }

        private OrganizationAggregate Organization { get; }
        public RealmId RealmId { get; }
        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;
        public IReadOnlyCollection<ConfigurationSection> ConfigurationSections => configurationSections.Values;

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
            SchemaTypeId schemaTypeId,
            string path,
            string description)
        {
            path.EnsureValue(nameof(path));
            var configurationSectionId = ConfigurationSectionId.NewConfigurationSectionId(configurationSectionName);

            // make sure the configuration section doesn't already exist
            habitats.Keys.EnsureNameDoesntAlreadyExist(configurationSectionId);

            // lazy - throws an exception if type doesn't exist.
            Organization.GetSchema(schemaTypeId);

            // create and raise the event
            var evt = new AddedConfigurationSectionToRealmEvent(
                Organization.OrganizationId,
                RealmId,
                configurationSectionId,
                new SchemaTypeId("todo", "todo"),
                path,
                description);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedConfigurationSectionToRealmEvent, ConfigurationSection>(evt);
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