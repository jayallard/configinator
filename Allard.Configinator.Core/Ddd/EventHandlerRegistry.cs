using System;
using System.Collections.Generic;

namespace Allard.Configinator.Core.Ddd
{
    public class EventHandlerRegistry
    {
        private readonly Dictionary<Type, EventHandlerRegistryBuilder.Executor> handlers;
        private readonly List<DomainEvent> events = new();

        public EventHandlerRegistry(Dictionary<Type, EventHandlerRegistryBuilder.Executor> handlers)
        {
            this.handlers = handlers;
        }

        /// <summary>
        /// Used for actions, and by the repo.
        /// </summary>
        /// <param name="evt"></param>
        private void ApplyEvent(DomainEvent evt)
        {
            var executor = handlers[evt.GetType()];
            executor.Execute(evt);
        }
        
        /// <summary>
        /// Used to return responses to the calling methods (a convenience).
        /// </summary>
        /// <param name="evt"></param>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        private TResponse Apply<TResponse>(DomainEvent evt)
        {
            var executor = handlers[evt.GetType()];
            return (TResponse)executor.Execute(evt);
        }

        public TResponse Raise<TEvent, TResponse>(TEvent evt)
            where TEvent : DomainEvent
        {
            events.Add(evt);
            return Apply<TResponse>(evt);
        }

        public void Raise<TEvent>(TEvent evt)
            where TEvent : DomainEvent
        {
            events.Add(evt);
            ApplyEvent(evt);
        }
    }
}