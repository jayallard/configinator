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
        List<ValidationFailure> ValidationFailures,
        JsonDocument Value);

    public class GetDetailedValueResponse
    {
        public ValueDetail Value { get; set; }
        public List<HabitatDetails> Habitats { get; } = new();

        [DebuggerDisplay("HabitatId={HabitatId}, Exists={Exists}, Failure Count={ValidationFailures.Count}")]
        public class HabitatDetails
        {
            public string HabitatId { get; set; }
            public List<ValidationFailure> ValidationFailures { get; } = new();
            public bool Exists { get; set; }
            public string ConfigurationValue { get; set; }
        }

        [DebuggerDisplay("Name={Name}")]
        public class ValueDetail
        {
            public string Name { get; set; }
            public List<PropertyValue> Properties { get; } = new();
            public List<ValueDetail> Objects { get; } = new();
        }

        [DebuggerDisplay("Name={Name}, Value={ResolvedValue}")]
        public class PropertyValue
        {
            public string Name { get; set; }
            public string ResolvedValue { get; set; }
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
            public string HabitatId { get; set; }
            public string Value { get; set; }
        }
    }


    public record SetValueRequest(
        ConfigurationId ConfigurationId,
        string SettingsPath,
        JsonDocument Value);

    public record SetValueResponse(List<SetValueResponseHabitat> Habitats);

    public record SetValueResponseHabitat(bool Changed, bool Saved, string HabitatId,
        List<ValidationFailure> ValidationFailures);
}