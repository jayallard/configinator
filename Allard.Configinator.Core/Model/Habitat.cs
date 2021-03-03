namespace Allard.Configinator.Core.Model
{
    public class Habitat
    {
        private readonly Realm realm;
        public HabitatId Id { get; }
        internal Habitat(Realm realm, HabitatId id)
        {
            this.realm = realm;
            Id = id;
        }
    }

    public record HabitatId(string Id, string Name);
}