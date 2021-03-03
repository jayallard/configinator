using System;
using System.Collections.Generic;

namespace Allard.Configinator.Core.Ddd
{
    public class EventHandlerRegistryBuilder
    {
        private readonly Dictionary<Type, Executor> handlers = new();

        public abstract class Executor
        {
            public object Execute(DomainEvent evt)
            {
                return ReallyExecute(evt);
            }

            protected abstract object ReallyExecute(object evt);
        }

        public class ExecutorT<TEvent, TResponse> : Executor where TEvent : DomainEvent
        {
            private readonly Func<TEvent, TResponse> method;

            public ExecutorT(Func<TEvent, TResponse> method)
            {
                this.method = method;
            }

            protected override object ReallyExecute(object evt)
            {
                return method((TEvent) evt);
            }
        }

        public EventHandlerRegistryBuilder Register<TEvent, TResponse>(Func<TEvent, TResponse> action)
            where TEvent : DomainEvent
            where TResponse : class
        {
            if (handlers.ContainsKey(typeof(TEvent)))
            {
                throw new Exception("handler already registered");
            }

            var executor = new ExecutorT<TEvent, TResponse>(action);
            handlers[typeof(TEvent)] = executor;
            return this;
        }

        private record Nothing();

        public EventHandlerRegistryBuilder Register<TEvent>(Action<TEvent> action)
            where TEvent : DomainEvent
        {
            if (handlers.ContainsKey(typeof(TEvent)))
            {
                throw new Exception("handler already registered");
            }

            handlers[typeof(TEvent)] = new ExecutorT<TEvent, Nothing>(evt =>
            {
                action(evt);
                return null;
            });
            return this;
        }

        public EventHandlerRegistry Build()
        {
            return new(handlers);
        }
    }
}