namespace Allard.Configinator.Core.Model
{
    // todo: why isn't this a record?

    public class Habitat
    {
        public Realm Realm { get; }
        public HabitatId HabitatId { get; }
        public Habitat BaseHabitat { get; }

        internal Habitat(HabitatId habitatId, Realm realm, Habitat baseHabitat = null)
        {
            Realm = realm;
            BaseHabitat = baseHabitat;
            HabitatId = habitatId;
        }
        
    }
}