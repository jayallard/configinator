using System;

namespace Allard.Configinator.Core.Ddd
{
    public abstract record DomainEvent
    {
        public string EventId { get; } = Guid.NewGuid().ToString();
        public DateTime EventDate { get; } = DateTime.UtcNow;
        public string EventName => GetType().Name;
    }
}