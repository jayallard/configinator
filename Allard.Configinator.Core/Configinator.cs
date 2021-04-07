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
    public class Configinator : IConfiginator
    {
        private readonly IConfigStore configStore;

        public Configinator(OrganizationAggregate org, IConfigStore configStore)
        {
            Organization = org.EnsureValue(nameof(org));
            this.configStore = configStore.EnsureValue(nameof(configStore));
        }

        public OrganizationAggregate Organization { get; }

        public async Task<SetValueResponse> SetValueAsync(SetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);

            async Task<ObjectDto> ConfigResolver(IHabitat h)
            {
                // delegate to get value from config store
                return (await GetValueFromConfigstore(cs, h)).ToObjectDto();
            }

            // the model represents what the configuration value
            // looks like. it has structure and data types (currently only string)
            var model = StructureBuilder.ToStructure(cs);

            // does the heavy lifting of copying values from a habitat to
            // the descendent habitats.
            var resolver = new HabitatValueResolver(model, ConfigResolver, habitat);

            // load the existing values from config store into the trackers
            await resolver.LoadExistingValues();

            // overwrite the config store value with the new value.
            // this is the value that the user is saving.
            var newValue = request.Value.ToObjectDto();
            resolver.OverwriteValue(habitat, newValue, request.SettingsPath);

            // the state object keeps track of things.
            var state = resolver
                .VersionedHabitats
                .Select(v => v.Versions.Last())
                .Select(h => new State
                {
                    Habitat = realm.GetHabitat(h.VersionName),
                    IsChanged = h.IsChanged,
                    Object = h.ToObjectDto(),
                    IsSaved = false
                })
                .ToList();

            // validate every habitat.
            foreach (var s in state)
            {
                var failures = new ConfigurationValidator(cs, Organization.SchemaTypes)
                    .Validate(s.Habitat.HabitatId, s.Object)
                    .ToList();
                s.Failures.AddRange(failures);
            }

            // if there are any errors, then can't save.
            // if nothing changed, then no point in saving.
            if (state.Any(s => !s.CanSave || !s.IsChanged)) return ToResponse(state);

            // save
            foreach (var s in state.Where(s => s.CanSave))
            {
                // todo: change config store to take a list in case it can do them all in a transaction
                var json = s.Object.ToJson();
                var path = OrganizationAggregate.GetConfigurationPath(cs, s.Habitat);
                var r = new SetConfigStoreValueRequest(path, json);
                await configStore.SetValueAsync(r);
                s.IsSaved = true;
            }

            return ToResponse(state);
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            var realm = Organization.GetRealmByName(request.ConfigurationId.RealmId);
            var habitat = realm.GetHabitat(request.ConfigurationId.HabitatId);
            var cs = realm.GetConfigurationSection(request.ConfigurationId.SectionId);
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var value = await configStore.GetValueAsync(path);
            var doc = value.Exists
                ? value.Value
                : StructureBuilder.ToStructure(cs).ToJson();
            
            // if validation is requested
            if (request.Validate)
            {
                var v = value.Exists
                    ? value.Value.ToObjectDto()
                    : StructureBuilder.ToStructure(cs);
                var results = new ConfigurationValidator(cs, Organization.SchemaTypes).Validate(habitat.HabitatId, v);
                return new GetValueResponse(request.ConfigurationId, value.Exists, results.ToList(), doc);
            }
            
            // no validation - just return it
            var response = new GetValueResponse(request.ConfigurationId, value.Exists, null, doc);
            return response;
        }

        private static SetValueResponse ToResponse(IEnumerable<State> states)
        {
            var habitats = states
                .Select(h =>
                    new SetValueResponseHabitat(h.IsChanged, h.IsSaved, h.Habitat.HabitatId.Id,
                        h.Failures))
                .ToList();
            return new SetValueResponse(habitats);
        }

        /// <summary>
        ///     Retrieve a value from the config store, if it exists.
        ///     If it doesn't exist, returns an empty document.
        /// </summary>
        /// <param name="cs">The configuration section of the value.</param>
        /// <param name="habitat">The habitat of the value.</param>
        /// <returns></returns>
        private async Task<JsonDocument> GetValueFromConfigstore(
            ConfigurationSection cs, IHabitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var (_, value, exists) = await configStore.GetValueAsync(path);
            return exists
                ? value
                : JsonDocument.Parse("{}");
        }

        private class State
        {
            public ObjectDto Object { get; init; }
            public IHabitat Habitat { get; init; }
            public List<ValidationFailure> Failures { get; } = new();
            public bool IsChanged { get; init; }
            public bool IsSaved { get; set; }
            public bool CanSave => IsChanged && Failures.Count == 0;
        }
    }
}