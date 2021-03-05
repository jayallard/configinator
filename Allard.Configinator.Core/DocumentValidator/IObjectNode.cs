using System.Collections.Generic;

namespace Allard.Configinator.Core.DocumentValidator
{
    public interface IObjectNode
    {
        IEnumerable<IPropertyNode> GetPropertyValues();
        IEnumerable<IObjectNode> GetObjectNodes();
        
        string Name { get; }
    }
}