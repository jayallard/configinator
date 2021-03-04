namespace Allard.Configinator.Core.Model
{
    public abstract record ModelMemberId
    {
        public string Id { get; }
        public string Name { get; }
        
        protected ModelMemberId(string id, string name)
        {
            Id = id.EnsureValue(nameof(id));
            Name = name.ToNormalizedMemberName(nameof(name));
        }
    }
}