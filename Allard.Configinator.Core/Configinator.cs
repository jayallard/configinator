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
                return (await GetValueFromConfigstore(cs, h)).ToObjectDto();
            }

            var model = StructureBuilder.ToStructure(cs);
            var resolver = new HabitatValueResolver(model, ConfigResolver, habitat);
            await resolver.LoadExistingValues();
            var newValue = request.Value.ToObjectDto();
            resolver.OverwriteValue(habitat, newValue, request.SettingsPath);

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

            // validate
            foreach (var s in state)
            {
                var failures = new ConfigurationValidator(cs, Organization.SchemaTypes)
                    .Validate(s.Habitat.HabitatId, s.Object)
                    .ToList();
                s.Failures.AddRange(failures);
            }

            // if there are any errors, then can't save.
            // exit.
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


            // get the bases and the specific value, then merge.
            //var configsToGet = GetValueFromConfigstore(cs, habitat);
            //var habitats = GetHabitatTree(habitat.HabitatId, realm.Habitats.ToList());

            return null;
            // var toMerge = (await configsToGet).ToList();
            // var model = structureModelBuilder.ToStructureModel(cs);
            // var merged = await DocMerger3.Merge(model, toMerge.Select(m => m.Item1));
            // var value = GetDeepValue(merged, request.ValuePath, habitat.HabitatId);
            // var anyExists = toMerge.Any(m => m.Item2.Exists);
            // return new GetValueResponse(request.ConfigurationId, anyExists, value, merged);
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