using Allard.Configinator.Core.Ddd;

namespace Allard.Configinator.Core.Events
{
    public record RealmCreatedEvent(string Name, string id) : DomainEvent;
}