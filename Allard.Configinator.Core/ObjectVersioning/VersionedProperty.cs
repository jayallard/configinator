using System.Diagnostics;

namespace Allard.Configinator.Core.ObjectVersioning
{
    [DebuggerDisplay("Name={Name}, VersionName={VersionName}, Value={Value}")]
    public class VersionedProperty : IProperty
    {
        public VersionedProperty(
            string versionName,
            string name,
            string value,
            VersionedNode parent)
        {
            Name = name;
            VersionName = versionName;
            OriginalValue = Value = value;
            Parent = parent;
        }

        private bool IsSet { get; set; }
        public bool IsChanged => IsSet && !string.Equals(OriginalValue, Value);
        public string VersionName { get; }
        public string OriginalValue { get; }
        private VersionedNode Parent { get; }
        public VersionedProperty PreviousVersion { get; internal set; }
        public VersionedProperty NextVersion { get; internal set; }
        public string Name { get; }
        public string Value { get; private set; }
        public void SetValue(string value)
        {
            Value = value;
            IsSet = true;
        }
    }
}