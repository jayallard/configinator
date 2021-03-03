using System;
using System.Collections.Generic;

namespace Allard.Configinator.Core.Ddd
{
    public class EventHandlerRegistry
    {
        private readonly Dictionary<Type, Func<DomainEvent, object>> handlers;

        public EventHandlerRegistry(Dictionary<Type, Func<DomainEvent, object>> handlers)
        {
            this.handlers = handlers;
        }

        public object Raise<T>(T evt) where T : DomainEvent
        {
            return handlers[typeof(T)](evt);
        }
    }
}