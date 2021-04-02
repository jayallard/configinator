using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    [DebuggerDisplay("Name={Name}, VersionName={VersionName}")]
    public class VersionedObject
    {
        public VersionedObject(
            string name,
            string versionName,
            IEnumerable<VersionedProperty> properties,
            IEnumerable<VersionedObject> objects,
            VersionedObject parentObject)
        {
            Name = name;
            VersionName = versionName;
            Properties = properties.ToList().AsReadOnly();
            Objects = objects.ToList().AsReadOnly();
            Parent = parentObject;
            propertiesByName = Properties.ToDictionary(p => p.Name);
            objectsByName = Objects.ToDictionary(o => o.Name);
        }

        private readonly Dictionary<string, VersionedProperty> propertiesByName;
        private readonly Dictionary<string, VersionedObject> objectsByName;

        public string Name { get; }
        public string VersionName { get; }
        public IReadOnlyCollection<VersionedProperty> Properties { get; }
        public IReadOnlyCollection<VersionedObject> Objects { get; }
        public VersionedObject Parent { get;  }
        public VersionedObject PreviousVersion { get; internal set; }
        public VersionedObject NextVersion { get; internal set; }

        public VersionedProperty GetProperty(string name)
        {
            return propertiesByName[name];
        }

        public VersionedObject GetObject(string name)
        {
            return objectsByName[name];
        }
    }
}