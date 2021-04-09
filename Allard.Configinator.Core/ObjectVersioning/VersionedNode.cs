using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    [DebuggerDisplay("Name={Name}, VersionName={VersionName}")]
    public class VersionedNode
    {
        private readonly Dictionary<string, VersionedNode> nodesByName;

        private readonly Dictionary<string, VersionedProperty> propertiesByName;

        public VersionedNode(
            string name,
            string versionName,
            IEnumerable<VersionedProperty> properties,
            IEnumerable<VersionedNode> nodes,
            VersionedNode parentNode)
        {
            Name = name;
            VersionName = versionName;
            Properties = properties.ToList().AsReadOnly();
            Objects = nodes.ToList().AsReadOnly();
            Parent = parentNode;
            propertiesByName = Properties.ToDictionary(p => p.Name);
            nodesByName = Objects.ToDictionary(o => o.Name);
        }

        public string Name { get; }
        public string VersionName { get; }
        public IReadOnlyCollection<VersionedProperty> Properties { get; }
        public IReadOnlyCollection<VersionedNode> Objects { get; }
        private VersionedNode Parent { get; }
        public VersionedNode PreviousVersion { get; internal set; }
        public VersionedNode NextVersion { get; internal set; }

        public bool IsChanged => propertiesByName.Values.Any(p => p.IsChanged)
                                 || nodesByName.Values.Any(p => p.IsChanged);

        public VersionedProperty GetProperty(string name)
        {
            return propertiesByName[name];
        }

        public VersionedNode GetNode(string name)
        {
            return nodesByName[name];
        }
    }
}