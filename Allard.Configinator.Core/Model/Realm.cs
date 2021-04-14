using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Allard.Configinator.Core.Events;

namespace Allard.Configinator.Core.Model
{
    [DebuggerDisplay("RealmId={RealmId.Id}")]
    public class Realm : IRealm
    {
        private readonly Dictionary<SectionId, ConfigurationSection> configurationSections = new();
        private readonly Dictionary<HabitatId, IHabitat> habitats = new();
        private readonly Dictionary<string, RealmVariable> variables = new();

        public Realm(OrganizationAggregate organization, RealmId realmId)
        {
            Organization = organization;
            RealmId = realmId;
        }

        public OrganizationAggregate Organization { get; }
        public RealmId RealmId { get; }
        public IEnumerable<ConfigurationSection> ConfigurationSections => configurationSections.Values;
        public IReadOnlyCollection<IHabitat> Habitats => habitats.Values;

        public IHabitat GetHabitat(string habitatId)
        {
            var id = new HabitatId(habitatId);
            if (habitats.TryGetValue(id, out var habitat)) return habitat;
            throw ModelExceptions.HabitatDoesntExist(habitatId);
        }

        public ConfigurationSection GetConfigurationSection(string sectionId)
        {
            var id = new SectionId(sectionId);
            if (configurationSections.TryGetValue(id, out var cs)) return cs;
            throw ModelExceptions.ConfigurationSectionDoesntExists(id.Id);
        }

        public Realm AddVariable(RealmVariable variable)
        {
            var id = new SectionId(variable.ConfigurationSectionId);
            if (!configurationSections.ContainsKey(id))
            {
                throw ModelExceptions.ConfigurationSectionDoesntExists(id.Id);
            }

            if (variables.ContainsKey(variable.Name))
            {
                throw ModelExceptions.RealmVariableAlreadyExists(variable.Name);
            }

            if (string.IsNullOrWhiteSpace(variable.ConfigPath) || variable.ConfigPath
                .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length == 0)
            {
                throw new InvalidOperationException("Invalid Configuration Path: " + variable.ConfigPath);
            }

            var cs = GetConfigurationSection(variable.ConfigurationSectionId);
            var structure = StructureBuilder.ToStructure(cs);
            if (!structure.Exists(variable.ConfigPath))
            {
                throw new InvalidOperationException("Configuration Path doesn't exist: " + variable.ConfigPath);
            }

            var node = structure.FindNode(variable.ConfigPath);
            
            return this;
        }

        /// <summary>
        ///     Used by the event handler.
        /// </summary>
        /// <param name="habitat"></param>
        internal void AddHabitat(IHabitat habitat)
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

            if (!properties.Any()) throw new InvalidOperationException("At least one property is required.");

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

        public IHabitat AddHabitat(string habitatId, string baseHabitatId = null)
        {
            return AddHabitat(new HabitatId(habitatId), baseHabitatId == null ? null : new HabitatId(baseHabitatId));
        }

        public Habitat AddHabitat(HabitatId habitatId, HabitatId baseHabitatId = null)
        {
            // make sure habitat doesn't already exist.
            habitats.Keys.EnsureIdDoesntExist(habitatId);

            if (baseHabitatId != null && !habitats.ContainsKey(baseHabitatId))
            {
                throw new InvalidOperationException("BaseHabitat doesn't exist: " + baseHabitatId.Id);
            }

            // create and raise the event.
            var evt = new AddedHabitatToRealmEvent(Organization.OrganizationId, RealmId, habitatId, baseHabitatId);
            return Organization
                .EventHandlerRegistry
                .Raise<AddedHabitatToRealmEvent, Habitat>(evt);
        }
    }
}