using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.Model
{
    public class Habitat
    {
        private List<Habitat> bases;
        private readonly Realm realm;
        public HabitatId HabitatId { get; }
        public IReadOnlyCollection<Habitat> Bases => bases.AsReadOnly();
        
        internal Habitat(HabitatId habitatId, Realm realm, IEnumerable<Habitat> bases)
        {
            this.realm = realm;
            this.bases = bases.ToList();
            HabitatId = habitatId;
        }
    }

    public record HabitatId(string Id, string Name) : ModelMemberId(Id, Name)
    {
        public static HabitatId NewHabitatId(string name)
        {
            return new (Guid.NewGuid().ToString(), name);
        }
    }
}