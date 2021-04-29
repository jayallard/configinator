using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Blazor.Client.Services
{
    public class StupidEventBus : IEventBus
    {
        private readonly List<Subscriber> subscribers = new();

        public void Subscribe(Func<object, bool> condition, Action<object> action)
        {
            subscribers.Add(new Subscriber(condition, action));
        }

        public void Publish(object evt)
        {
            foreach (var sub in subscribers.Where(s => s.Condition(evt))) sub.Action(evt);
        }

        private record Subscriber(Func<object, bool> Condition, Action<object> Action);
    }
}