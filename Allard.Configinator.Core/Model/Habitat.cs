using System.Collections.Generic;

namespace Allard.Configinator.Core.Model
{
    // todo: why isn't this a record?

    public interface IHabitat
    {
        IRealm Realm { get; }
        HabitatId HabitatId { get; }
        IHabitat BaseHabitat { get; }
        IEnumerable<IHabitat> Children { get; }
    }

    public class Habitat : IHabitat
    {
        private readonly List<IHabitat> children = new();
        internal Habitat(HabitatId habitatId, IRealm realm, IHabitat baseHabitat = null)
        {
            Realm = realm;
            BaseHabitat = baseHabitat;
            HabitatId = habitatId;
        }

        public IRealm Realm { get; }
        public HabitatId HabitatId { get; }
        public IHabitat BaseHabitat { get; }
        public IEnumerable<IHabitat> Children => children.AsReadOnly();
        internal void AddChild(IHabitat child)
        {
            children.Add(child);
        }
    }
}