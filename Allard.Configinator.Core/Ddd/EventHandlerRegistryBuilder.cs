using System;
using System.Collections.Generic;

namespace Allard.Configinator.Core.Ddd
{
    public class EventHandlerRegistryBuilder
    {
        private readonly Dictionary<Type, Func<DomainEvent, object>> handlers = new();

        public EventHandlerRegistryBuilder Register<TEvent, TResponse>(Func<DomainEvent, TResponse> action)
            where TResponse : class
        {
            if (handlers.ContainsKey(typeof(TEvent)))
            {
                throw new Exception("handler already registered");
            }

            handlers[typeof(TEvent)] = action;
            return this;
        }

        public EventHandlerRegistryBuilder Register<TEvent>(Action<DomainEvent> action)
        {
            if (handlers.ContainsKey(typeof(TEvent)))
            {
                throw new Exception("handler already registered");
            }

            handlers[typeof(TEvent)] = evt =>
            {
                action(evt);
                return null;
            };
            return this;
        }

        public EventHandlerRegistry Build()
        {
            return new(handlers);
        }
    }
}