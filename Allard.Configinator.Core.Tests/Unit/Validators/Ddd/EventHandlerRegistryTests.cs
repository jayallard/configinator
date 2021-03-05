using Allard.Configinator.Core.Ddd;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Validators.Ddd
{
    public class EventHandlerRegistryTests
    {
        [Fact]
        public void EventHandlerAction()
        {
            var r1 = string.Empty;
            var r2 = string.Empty;
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent>(_ => r1 = "boo yea")
                .Register<SomethingElseEvent>(_ => r2 = "santa claus")
                .Build();

            registry.Raise(new SomethingEvent());
            registry.Raise(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        [Fact(Skip = "Demonstrating a known issue.")]
        public void IfTypesAreWrong()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(_ => string.Empty)
                .Build();
            registry.Raise<SomethingEvent, int>(new SomethingEvent());
        }

        [Fact]
        public void EvenHandlerFunction()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(_ => "boo yea")
                .Register<SomethingElseEvent, string>(_ => "santa claus")
                .Build();

            var r1 = registry.Raise<SomethingEvent, string>(new SomethingEvent());
            var r2 = registry.Raise<SomethingElseEvent, string>(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }
        public record SomethingEvent : DomainEvent;

        public record SomethingElseEvent : DomainEvent;

    }
}