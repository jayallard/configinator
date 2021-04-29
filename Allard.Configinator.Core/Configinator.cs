using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security;
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
            return await new HabitatValueSetter(Organization, configStore).SetValueAsync(request);
        }

        public async Task<GetDetailedValueResponse> GetValueDetailAsync(GetValueRequest request)
        {
            var (configurationId, needsValidation, _) = request;
            var realm = Organization.GetRealmById(configurationId.RealmId);
            var habitat = realm.GetHabitat(configurationId.HabitatId);
            var cs = realm.GetConfigurationSection(configurationId.SectionId);

            var result = new GetDetailedValueResponse();
            var currentHabitat = habitat;
            var validator = new ConfigurationValidator(cs.Properties);
            var dtos = new List<Node>();
            var modelDto = cs.ToStructure();
            var modelJson = modelDto.ToJson().RootElement.ToString();
            while (currentHabitat != null)
            {
                var path = OrganizationAggregate.GetConfigurationPath(cs, currentHabitat);
                var (_, currentValue, currentExists) = await configStore.GetValueAsync(path);
                var json = currentExists
                    ? currentValue.RootElement.ToString()
                    : modelJson;
                var dto = currentExists
                    ? currentValue.ToNode()
                    : modelDto;
                dtos.Add(dto);

                var habitatDetails = new GetDetailedValueResponse.HabitatDetails
                {
                    Exists = currentExists,
                    ConfigurationValue = json,
                    HabitatId = currentHabitat.HabitatId.Id
                };
                result.Habitats.Add(habitatDetails);
                if (needsValidation)
                {
                    var toValidate = currentExists
                        ? dto
                        : modelDto;
                    var configId = new ConfigurationId(
                        realm.Organization.OrganizationId.Id,
                        realm.RealmId.Id,
                        cs.SectionId.Id,
                        currentHabitat.HabitatId.Id);

                    // Convert SchemaValidationFailure failures to ConfiginatorValidationFailure.
                    // ConfiginatorValidationFailure has the id.
                    habitatDetails.ValidationFailures.AddRange(
                        validator
                            .Validate(toValidate)
                            .Select(v =>
                                new ConfiginatorValidationFailure(configId, v.Path, v.Code, v.Message)));
                }

                currentHabitat = currentHabitat.BaseHabitat;
            }

            dtos.Reverse();
            var habitatIds = result.Habitats.Select(h => h.HabitatId).ToList();
            habitatIds.Reverse();
            result.Value = BuildDetailedValue(modelDto, dtos, habitatIds);
            return result;
        }

        private async void ResolveAll(ConfigurationSection cs)
        {
            var sectionModel = cs.ToStructure();
            var trackers = new VersionTrackers(sectionModel);

            var variableTasks = cs
                .Realm
                .GetVariables()
                .Select(v => new
                {
                    Variable = v,
                    VariableTask = 
                })


                var valuesPerHabitatTasks = cs
                .Realm
                .Habitats
                .Select(h => new
                {
                    Habitat = h,
                    ValueTask = GetValueFromConfigStore(cs, h)
                })
                .ToList();
            await Task.WhenAll(valuesPerHabitatTasks.Select(v => v.ValueTask));
            var valuesPerHabitat = valuesPerHabitatTasks
                .Select(t => new
                {
                    t.Habitat,
                    Value = t.ValueTask.Result.ToNode()
                })
                .ToDictionary(h => h.Habitat.HabitatId.Id);

            foreach (var h in cs.Realm.Habitats)
            {
                var habitatLine = new List<IHabitat>();
                var current = h;
                while (current != null)
                {
                    habitatLine.Add(current);
                    current = current.BaseHabitat;
                }

                for (var i = habitatLine.Count - 1; i >= 0; i--)
                {
                    var toAdd = habitatLine[i];
                    var versionName = "Habitat=" + toAdd.HabitatId.Id;
                    trackers.AddVersion(h.HabitatId.Id, versionName, valuesPerHabitat[toAdd.HabitatId.Id].Value);
                }
            }
        }

        public async Task<SetVariableResponse> SetVariable(SetVariableRequest request)
        {
            var realm = Organization.GetRealmById(request.RealmId);
            var variable = realm.GetVariable(request.VariableName);

            // store the variable value in the store.
            var path = OrganizationAggregate.GetVariablePath(realm, variable);
            var configRequest = new SetConfigStoreValueRequest(path, request.Value);
            await configStore.SetValueAsync(configRequest);
            
            // apply the variable value to all habitats.
            var assignmentsBySection = variable.Assignments.GroupBy(g => g.SectionId).ToList();
            foreach (var assignment in assignmentsBySection)
            {
                var section = realm.GetConfigurationSection(assignment.Key);
                await ResolveAll(section);
            }

            return new();
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            var (configurationId, validate, _) = request;
            var realm = Organization.GetRealmById(configurationId.RealmId);
            var habitat = realm.GetHabitat(configurationId.HabitatId);
            var cs = realm.GetConfigurationSection(configurationId.SectionId);
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var (_, configDocument, configExists) = await configStore.GetValueAsync(path);
            var doc = configExists
                ? configDocument
                : cs.ToStructure().ToJson();

            // if validation is requested
            if (validate)
            {
                var v = configExists
                    ? configDocument.ToNode()
                    : cs.ToStructure();
                var results = new ConfigurationValidator(cs.Properties).Validate(v);
                return new GetValueResponse(configurationId, configExists, results.ToList(), doc);
            }

            // no validation - just return it
            var response = new GetValueResponse(configurationId, configExists, null, doc);
            return response;
        }

        private static GetDetailedValueResponse.ValueDetail BuildDetailedValue(
            Node model,
            IReadOnlyCollection<Node> dtos,
            IReadOnlyList<string> habitatIds)
        {
            var detail = new GetDetailedValueResponse.ValueDetail();
            AddObject(model, detail, dtos);

            void AddObject(Node currentModel, GetDetailedValueResponse.ValueDetail currentDetail,
                IReadOnlyCollection<Node> values)
            {
                // iterate the objects
                foreach (var modelObject in currentModel.Objects)
                {
                    var nextObject = new GetDetailedValueResponse.ValueDetail
                    {
                        Name = modelObject.Name
                    };
                    currentDetail.Objects.Add(nextObject);
                    var nextValues = values
                        .Select(v => v.GetObject(modelObject.Name))
                        .ToList();
                    AddObject(modelObject, nextObject, nextValues);
                }

                foreach (var p in currentModel.Properties)
                {
                    var valuesPerHabitat = values
                        .Select((t, i) => new GetDetailedValueResponse.HabitatValue
                        {
                            HabitatId = habitatIds[i],
                            Value = t.GetProperty(p.Name).Value
                        }).ToList();

                    currentDetail.Properties.Add(new GetDetailedValueResponse.PropertyValue
                        {
                            Name = p.Name,
                            ResolvedValue = valuesPerHabitat.Last().Value
                        }
                        .AddValues(valuesPerHabitat)
                    );
                }
            }

            return detail;
        }

        /// <summary>
        ///     Retrieve a value from the config store, if it exists.
        ///     If it doesn't exist, returns an empty document.
        /// </summary>
        /// <param name="cs">The configuration section of the value.</param>
        /// <param name="habitat">The habitat of the value.</param>
        /// <returns></returns>
        private async Task<JsonDocument> GetValueFromConfigStore(
            ConfigurationSection cs, IHabitat habitat)
        {
            var path = OrganizationAggregate.GetConfigurationPath(cs, habitat);
            var (_, value, exists) = await configStore.GetValueAsync(path);
            return exists
                ? value
                : JsonDocument.Parse("{}");
        }
    }
}