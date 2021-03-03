using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    public class Habitat
    {
        private List<Habitat> bases;
        private readonly Realm realm;
        public HabitatId Id { get; }
        public IReadOnlyCollection<Habitat> Bases => bases.AsReadOnly();
        
        internal Habitat(HabitatId id, Realm realm, IEnumerable<Habitat> bases)
        {
            this.realm = realm;
            this.bases = bases.ToList();
            Id = id;
        }
    }

    public record HabitatId(string Id, string Name);
}