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
            return await new HabitatValueSetter(Organization, configStore).SetValueAsync(request);
        }

        public async Task<GetDetailedValueResponse> GetValueDetailAsync(GetValueRequest request)
        {
            var (configurationId, needsValidation, _) = request;
            var realm = Organization.GetRealmByName(configurationId.RealmId);
            var habitat = realm.GetHabitat(configurationId.HabitatId);
            var cs = realm.GetConfigurationSection(configurationId.SectionId);

            var result = new GetDetailedValueResponse();
            var current = habitat;
            var validator = new ConfigurationValidator(cs, Organization.SchemaTypes);
            var dtos = new List<Node>();
            var modelDto = cs.ToStructure();
            var modelJson = modelDto.ToJson().RootElement.ToString();
            while (current != null)
            {
                var path = OrganizationAggregate.GetConfigurationPath(cs, current);
                var (_, currentValue, currentExists) = await configStore.GetValueAsync(path);
                var json = currentExists
                    ? currentValue.RootElement.ToString()
                    : modelJson;
                var dto = currentExists
                    ? currentValue.ToObjectDto()
                    : modelDto;
                dtos.Add(dto);

                var habitatDetails = new GetDetailedValueResponse.HabitatDetails
                {
                    Exists = currentExists,
                    ConfigurationValue = json,
                    HabitatId = current.HabitatId.Id
                };
                result.Habitats.Add(habitatDetails);
                if (needsValidation)
                {
                    var toValidate = currentExists
                        ? dto
                        : modelDto;
                    habitatDetails.ValidationFailures.AddRange(validator.Validate(habitat.HabitatId, toValidate));
                }

                current = current.BaseHabitat;
            }

            dtos.Reverse();
            var habitatIds = result.Habitats.Select(h => h.HabitatId).ToList();
            habitatIds.Reverse();
            result.Value = BuildDetailedValue(modelDto, dtos, habitatIds);
            return result;
        }

        public async Task<SetVariableResponse> SetVariable(SetVariableRequest request)
        {
            new ConfigurationValidator()
            return new();
        }

        public async Task<GetValueResponse> GetValueAsync(GetValueRequest request)
        {
            var (configurationId, validate, _) = request;
            var realm = Organization.GetRealmByName(configurationId.RealmId);
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
                    ? configDocument.ToObjectDto()
                    : cs.ToStructure();
                var results = new ConfigurationValidator(cs, Organization.SchemaTypes).Validate(habitat.HabitatId, v);
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
        private async Task<JsonDocument> GetValueFromConfigstore(
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