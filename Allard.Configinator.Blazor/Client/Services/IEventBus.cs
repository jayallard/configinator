using System;

namespace Allard.Configinator.Blazor.Client.Services
{
    public interface IEventBus
    {
        void Subscribe(Func<object, bool> subscriber, Action<object> action);
        void Publish(object evt);
    }
}