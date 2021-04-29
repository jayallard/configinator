using System.Collections.Generic;

namespace Allard.Configinator.Core.Model
{
    public interface IRealm
    {
        IReadOnlyCollection<IHabitat> Habitats { get; }
    }
}