using System.Collections.Generic;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public interface IObject
    {
        public string Name { get; }
        public List<IObject> Objects { get; }
        public List<IProperty> Properties { get; }
    }
}