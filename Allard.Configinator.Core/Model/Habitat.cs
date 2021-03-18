using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    // todo: why isn't this a record?

    public class Habitat
    {
        private readonly List<Habitat> bases;
        public Realm Realm { get; }

        internal Habitat(HabitatId habitatId, Realm realm, IEnumerable<Habitat> bases)
        {
            Realm = realm;
            this.bases = bases.ToList();
            HabitatId = habitatId;
        }

        public HabitatId HabitatId { get; }
        public IReadOnlyCollection<Habitat> Bases => bases.AsReadOnly();
    }
}