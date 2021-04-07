using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.DocumentValidator;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Infrastructure
{
    public record GetValueRequest(ConfigurationId ConfigurationId, bool Validate = true, string ValuePath = null);

    public record GetValueResponse(
        ConfigurationId ConfigurationId,
        bool Exists,
        List<ValidationFailure> ValidationFailures,
        JsonDocument Value);

    public class GetDetailedValue
    {
        public ValueDetail Value { get; set; }
        public List<HabitatDetails> Habitats { get; } = new();

        public class HabitatDetails
        {
            public string HabitatId { get; set; }
            public List<ValidationFailure> ValidationFailures { get; } = new();
            public bool Exists { get; set; }
            public string ConfigurationValue { get; set; }
        }
        
        public class ValueDetail
        {
            public string Name { get; set; }
            public List<PropertyValue> Properties { get; } = new();
            public List<ValueDetail> Objects { get; } = new();
        }

        public class PropertyValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public List<HabitatValue> Values { get; } = new();

            public PropertyValue AddValues(IEnumerable<HabitatValue> values)
            {
                Values.AddRange(values);
                return this;
            }
        }

        public class HabitatValue
        {
            public string HabitatName { get; set; }
            public string Transition { get; set; }
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