using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    // todo: why isn't this a record?

    public class Habitat
    {
        private readonly List<Habitat> bases;
        private readonly Realm realm;

        internal Habitat(HabitatId habitatId, Realm realm, IEnumerable<Habitat> bases)
        {
            this.realm = realm;
            this.bases = bases.ToList();
            HabitatId = habitatId;
        }

        public HabitatId HabitatId { get; }
        public IReadOnlyCollection<Habitat> Bases => bases.AsReadOnly();
    }
}