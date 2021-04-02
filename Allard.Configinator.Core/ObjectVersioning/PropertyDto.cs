namespace Allard.Configinator.Core.ObjectVersioning
{
    public class PropertyDto
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public PropertyDto SetName(string name)
        {
            Name = name;
            return this;
        }

        public PropertyDto SetValue(string value)
        {
            Value = value;
            return this;
        }
        
        public PropertyDto Clone()
        {
            return new()
            {
                Name = Name,
                Value = Value
            };
        }
    }
}