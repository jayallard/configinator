using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public enum ObjectType
    {
        Object,
        String
    }

    [DebuggerDisplay("Name={Name}")]
    public class ObjectDto
    {
        public ObjectType ObjectType { get; init; } = ObjectType.Object;
        public string Name { get; set; }
        public List<ObjectDto> Items { get; } = new();

        public IEnumerable<ObjectDto> Properties => Items.Where(i => i.IsProperty());
        public IEnumerable<ObjectDto> Objects => Items.Where(i => i.IsObject());

        public string Value { get; set; }

        public static ObjectDto CreateString(string name, string value = null)
        {
            return new()
            {
                ObjectType = ObjectType.String,
                Name = name,
                Value = value
            };
        }

        public ObjectDto SetName(string name)
        {
            Name = name;
            return this;
        }

        public ObjectDto SetValue(string value)
        {
            Value = value;
            return this;
        }

        public ObjectDto AddString(string name, string value = null)
        {
            Items.Add(CreateString(name, value));
            return this;
        }

        public ObjectDto GetObject(string name)
        {
            return Items.Single(o => o.Name == name && o.IsObject());
        }

        public ObjectDto Add(ObjectDto obj)
        {
            Items.Add(obj);
            return this;
        }

        public bool ObjectExists(string name)
        {
            return Items.Any(o => o.Name == name && o.IsObject());
        }

        public bool PropertyExists(string name)
        {
            return Items.Any(o => o.Name == name && o.IsProperty());
        }

        public ObjectDto Add(IEnumerable<ObjectDto> objects)
        {
            Items.AddRange(objects);
            return this;
        }

        public ObjectDto GetProperty(string name)
        {
            return Items.Single(p => p.Name == name && p.IsProperty());
        }

        public ObjectDto Clone()
        {
            return new ObjectDto()
                .SetName(Name)
                .Add(Items?.Select(o => o.Clone()));
        }
    }
}