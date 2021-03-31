namespace Allard.Configinator.Core.Model
{
    // todo: why isn't this a record?

    public interface IHabitat
    {
        IRealm Realm { get; }
        HabitatId HabitatId { get; }
        IHabitat BaseHabitat { get; }
    }
    
    public class Habitat : IHabitat
    {
        public IRealm Realm { get; }
        public HabitatId HabitatId { get; }
        public IHabitat BaseHabitat { get; }

        internal Habitat(HabitatId habitatId, IRealm realm, IHabitat baseHabitat = null)
        {
            Realm = realm;
            BaseHabitat = baseHabitat;
            HabitatId = habitatId;
        }
        
    }
}