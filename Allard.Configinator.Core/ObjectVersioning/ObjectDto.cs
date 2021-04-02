using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public class ObjectDto
    {
        public string Name { get; set; }
        public List<ObjectDto> Objects { get; set; }
        public List<PropertyDto> Properties { get; set; }

        public ObjectDto GetObject(string name)
        {
            return Objects.Single(o => o.Name == name);
        }

        public PropertyDto GetProperty(string name)
        {
            return Properties.Single(p => p.Name == name);
        }
        
        public ObjectDto Clone()
        {
            return new ObjectDto
            {
                Name = Name,
                Objects = Objects?.Select(o => o.Clone()).ToList(),
                Properties = Properties?.Select(p => p.Clone()).ToList()
            };
        }
    }
}