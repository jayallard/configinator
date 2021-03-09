using System.Collections.Generic;

namespace Allard.Configinator.Core.DocumentValidator
{
    public interface IObjectNode
    {
        string Name { get; }
        IEnumerable<IPropertyNode> GetPropertyValues();
        IEnumerable<IObjectNode> GetObjectNodes();
    }
}