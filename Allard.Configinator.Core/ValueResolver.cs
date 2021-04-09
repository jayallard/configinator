using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.ObjectVersioning;

namespace Allard.Configinator.Core
{
    public class HabitatValueSetter
    {
        private readonly OrganizationAggregate organization;
        private readonly IConfigStore configStore;
        private SetValueRequest request;
        private Realm realm;
        private IHabitat habitat;
        private ConfigurationSection configurationSection;
        private Node configurationSectionModel;
        private HabitatValueResolver habitatValueResolver;
        private List<State> state;

        public HabitatValueSetter(OrganizationAggregate organization, IConfigStore configStore)
        {
            this.organization = organization;
            this.configStore = configStore;
        }

        public async Task<SetValueResponse> SetValueAsync(SetValueRequest request)
        {
            this.request = request;
            Setup();

            await LoadExistingValues();
            UpdateWithNewValue();
            CreateState();
            ValidateValues();
            await SaveValues();
            return ToResponse(state);
        }

        private async Task<Node> ConfigResolver(IHabitat h)
        {
            // delegate to get value from config store
            return (await GetValueFromConfigstore(h)).ToObjectDto();
        }

        private void Setup()
        {
            realm = organization.GetRealmByName(request.ConfigurationId.RealmId);
            habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            configurationSection = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            // the model represents what the configuration value
            // looks like. it has structure and data types (currently only string)
            configurationSectionModel = StructureBuilder.ToStructure(configurationSection);

            // does the heavy lifting of copying values from a habitat to
            // the descendent habitats.
            habitatValueResolver = new HabitatValueResolver(configurationSectionModel, ConfigResolver, habitat);
        }

        /// <summary>
        /// Gets the value for the habitat and all
        /// of its descendant habitats.
        /// </summary>
        /// <returns></returns>
        private async Task LoadExistingValues()
        {
            // load the existing values from config store into the trackers
            await habitatValueResolver.LoadExistingValues();
        }

        /// <summary>
        /// Applies the new value to the habitat.
        /// Changes will cascade down the the descendant habitats.
        /// </summary>
        private void UpdateWithNewValue()
        {
            // overwrite the config store value with the new value.
            // this is the value that the user is saving.
            var newValue = request.Value.ToObjectDto();
            habitatValueResolver.OverwriteValue(habitat, newValue, request.SettingsPath);
        }

        /// <summary>
        /// Validate the values for the habitat and all descendants.
        /// Validate even if it didn't change, just to be thorough.
        /// Manual changes to the config store may have invalidated
        /// the value.
        /// </summary>
        private void ValidateValues()
        {
            // validate every habitat.
            foreach (var s in state)
            {
                var failures = new ConfigurationValidator(configurationSection, organization.SchemaTypes)
                    .Validate(s.Habitat.HabitatId, s.Value)
                    .ToList();
                s.Failures.AddRange(failures);
            }
        }

        /// <summary>
        /// Save changed values to the config store.
        /// </summary>
        /// <returns></returns>
        private async Task SaveValues()
        {
            // save
            foreach (var s in state.Where(s => s.CanSave))
            {
                // todo: change config store to take a list in case it can do them all in a transaction
                var json = s.Value.ToJson();
                var path = OrganizationAggregate.GetConfigurationPath(configurationSection, s.Habitat);
                var r = new SetConfigStoreValueRequest(path, json);
                await configStore.SetValueAsync(r);
                s.IsSaved = true;
            }
        }

        /// <summary>
        /// The state objects are used to track
        /// success and failures of validations
        /// and save operations.
        /// </summary>
        private void CreateState()
        {
            // the state object keeps track of things.
            state = habitatValueResolver
                .VersionedHabitats
                .Select(v => v.Versions.Last())
                .Select(h => new State
                {
                    Habitat = realm.GetHabitat(h.VersionName),
                    IsChanged = h.IsChanged,
                    Value = h.ToObjectDto(),
                    IsSaved = false
                })
                .ToList();
        }

        /// <summary>
        /// Convert the state objects to response objects.
        /// Wraps it up and return them.
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        private static SetValueResponse ToResponse(IEnumerable<State> states)
        {
            var habitats = states
                .Select(h =>
                    new SetValueResponseHabitat(h.IsChanged, h.IsSaved, h.Habitat.HabitatId.Id,
                        h.Failures))
                .ToList();
            return new SetValueResponse(habitats);
        }

        private class State
        {
            public Node Value { get; init; }
            public IHabitat Habitat { get; init; }
            public List<ValidationFailure> Failures { get; } = new();
            public bool IsChanged { get; init; }
            public bool IsSaved { get; set; }
            public bool CanSave => IsChanged && Failures.Count == 0;
        }

        /// <summary>
        ///     Retrieve a value from the config store, if it exists.
        ///     If it doesn't exist, returns an empty document.
        /// </summary>
        /// <param name="habitat">The habitat of the value.</param>
        /// <returns></returns>
        private async Task<JsonDocument> GetValueFromConfigstore(IHabitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(configurationSection, habitat);
            var (_, value, exists) = await configStore.GetValueAsync(path);
            return exists
                ? value
                : JsonDocument.Parse("{}");
        }
    }
}