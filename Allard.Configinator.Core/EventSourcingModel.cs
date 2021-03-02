using System;
using System.Collections.Generic;

namespace Allard.Configinator.Core
{
    public class EventSourcingModel
    {
        public class EventHandlerRegistryBuilder
        {
            private readonly Dictionary<Type, Action<DomainEvent>> handlers = new();

            public EventHandlerRegistryBuilder Register<T>(Action<DomainEvent> action) where T : DomainEvent
            {
                if (handlers.ContainsKey(action.GetType()))
                {
                    throw new Exception("handler already registered");
                }

                handlers[typeof(T)] = action;
                return this;
            }


            public EventHandlerRegistry Build()
            {
                return new(handlers);
            }
        }

        public class EventHandlerRegistry
        {
            private readonly Dictionary<Type, Action<DomainEvent>> handlers;

            public EventHandlerRegistry(Dictionary<Type, Action<DomainEvent>> handlers)
            {
                this.handlers = handlers;
            }

            public void Raise<T>(T evt) where T : DomainEvent
            {
                handlers[typeof(T)](evt);
            }
        }


        public class Organization : Aggregate<OrganizationId>
        {
            private readonly EventHandlerRegistry registry;
            public OrganizationId Id { get; private set; }

            public Organization(OrganizationId id) : this()
            {
                registry.Raise(new OrganizationCreatedEvent(id));
            }

            private void HandleOrganizationCreated(OrganizationCreatedEvent evt)
            {
                throw new Exception("yay!");
            }

            private Organization()
            {
                registry = new EventHandlerRegistryBuilder()
                    .Register<OrganizationCreatedEvent>(e =>
                    {
                        var evt = (OrganizationCreatedEvent) e;
                        Id = evt.Id;
                    })
                    .Build();
            }
        }

        public record OrganizationId(string Id)
        {
            public static OrganizationId NewOrganizationId = new OrganizationId(Guid.NewGuid().ToString());
        }


        public abstract class Aggregate<T>
        {
        }

        public abstract record DomainEvent
        {
        }

        public record OrganizationCreatedEvent(OrganizationId Id) : DomainEvent
        {
        }
    }
}