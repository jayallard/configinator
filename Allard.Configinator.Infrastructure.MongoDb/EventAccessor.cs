using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Infrastructure.MongoDb
{
    public class EventAccessor
    {
        private static readonly FieldInfo RegistryField = GetRegistryFieldFromOrganization();
        private static readonly MethodInfo ApplyMethod = GetApplyMethod();
        private static readonly FieldInfo EventsField = GetEventsFieldFromEventHandlerRegistry();

        private static FieldInfo GetRegistryFieldFromOrganization()
        {
            var field = typeof(Organization)
                .GetField("registry", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"'registry' field doesn't exist in the {nameof(Organization)} type.");
            }

            return field;
        }

        private static FieldInfo GetEventsFieldFromEventHandlerRegistry()
        {
            var field = typeof(EventHandlerRegistry)
                .GetField("events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"'events' field doesn't exist in the {nameof(EventHandlerRegistryBuilder)} type.");
            }

            return field;
        }

        private static MethodInfo GetApplyMethod()
        {
            var method =
                typeof(EventHandlerRegistry).GetMethod("ApplyEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new InvalidOperationException(
                    "'ApplyEvent' method doesn't exist in the EventHandlerRegistry class.");
            }

            return method;
        }

        private readonly Organization organization;
        private readonly EventHandlerRegistry registry;

        public EventAccessor(Organization organization)
        {
            this.organization = organization;
            registry = (EventHandlerRegistry) RegistryField.GetValue(organization);
        }

        public void ApplyEvent(DomainEvent evt)
        {
            ApplyMethod.Invoke(registry, BindingFlags.Default, null, new object?[] {evt}, null);
        }

        public IEnumerable<DomainEvent> GetEvents()
        {
            return (List<DomainEvent>) EventsField.GetValue(registry);
        }

        public void ClearEvents()
        {
            ((List<DomainEvent>) EventsField.GetValue(registry)).Clear();
        }
    }
}