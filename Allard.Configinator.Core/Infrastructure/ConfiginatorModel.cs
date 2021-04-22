using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Allard.Configinator.Core.DocumentValidator;

namespace Allard.Configinator.Core.Infrastructure
{
    public record GetValueRequest(ConfigurationId ConfigurationId, bool Validate = true, string ValuePath = null);

    public record GetValueResponse(
        ConfigurationId ConfigurationId,
        bool Exists,
        List<SchemaValidationFailure> ValidationFailures,
        JsonDocument Value);

    public class GetDetailedValueResponse
    {
        public ValueDetail Value { get; set; }
        public List<HabitatDetails> Habitats { get; } = new();

        [DebuggerDisplay("HabitatId={HabitatId}, Exists={Exists}, Failure Count={ValidationFailures.Count}")]
        public class HabitatDetails
        {
            public string HabitatId { get; init; }
            public List<SchemaValidationFailure> ValidationFailures { get; } = new();
            public bool Exists { get; init; }
            public string ConfigurationValue { get; init; }
        }

        [DebuggerDisplay("Name={Name}")]
        public class ValueDetail
        {
            public string Name { get; init; }
            public List<PropertyValue> Properties { get; } = new();
            public List<ValueDetail> Objects { get; } = new();
        }

        [DebuggerDisplay("Name={Name}, Value={ResolvedValue}")]
        public class PropertyValue
        {
            public string Name { get; init; }
            public string ResolvedValue { get; init; }
            public List<HabitatValue> HabitatValues { get; } = new();

            public PropertyValue AddValues(IEnumerable<HabitatValue> values)
            {
                HabitatValues.AddRange(values);
                return this;
            }
        }

        [DebuggerDisplay("HabitatId={HabitatId}, Value={Value}")]
        public class HabitatValue
        {
            public string HabitatId { get; init; }
            public string Value { get; init; }
        }
    }


    public record SetValueRequest(
        ConfigurationId ConfigurationId,
        string SettingsPath,
        JsonDocument Value);

    public record SetValueResponse(List<SetValueResponseHabitat> Habitats);

    public record SetValueResponseHabitat(bool Changed, bool Saved, string HabitatId,
        List<SchemaValidationFailure> ValidationFailures);

    public record SetVariableRequest(string Name, JsonDocument Value);

    public record SetVariableResponse();
}