using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    [DebuggerDisplay("Name={Name}")]
    public class ObjectDto
    {
        public string Name { get; set; }
        public List<ObjectDto> Objects { get; } = new();
        public List<PropertyDto> Properties { get; } = new();

        public ObjectDto SetName(string name)
        {
            Name = name;
            return this;
        }

        public ObjectDto AddProperty(string name, string value = null)
        {
            Properties.Add(new PropertyDto {Name = name, Value = value});
            return this;
        }

        public ObjectDto AddProperties(IEnumerable<PropertyDto> properties)
        {
            Properties.AddRange(properties);
            return this;
        }

        public ObjectDto GetObject(string name)
        {
            return Objects.Single(o => o.Name == name);
        }

        public ObjectDto AddObject(ObjectDto obj)
        {
            Objects.Add(obj);
            return this;
        }

        public bool ObjectExists(string name)
        {
            return Objects.Any(o => o.Name == name);
        }

        public bool PropertyExists(string name)
        {
            return Properties.Any(p => p.Name == name);
        }

        public ObjectDto AddProperty(PropertyDto property)
        {
            Properties.Add(property);
            return this;
        }

        public ObjectDto AddObjects(IEnumerable<ObjectDto> objects)
        {
            Objects.AddRange(objects);
            return this;
        }

        public PropertyDto GetProperty(string name)
        {
            return Properties.Single(p => p.Name == name);
        }

        public ObjectDto Clone()
        {
            return new ObjectDto()
                .SetName(Name)
                .AddObjects(Objects?.Select(o => o.Clone()))
                .AddProperties(Properties?.Select(p => p.Clone()));
        }
    }
}