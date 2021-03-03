using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Events;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var id = OrganizationId.NewOrganizationId;
            var org = new Organization(id);
            org.Id.Should().Be(id);
        }

        [Fact]
        public void EventHandlerAction()
        {
            var r1 = string.Empty;
            var r2 = string.Empty;
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent>(e => r1 = "boo yea")
                .Register<SomethingElseEvent>(e => r2 = "santa claus")
                .Build();

            registry.Raise(new SomethingEvent());
            registry.Raise(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        [Fact]
        public void EvenHandlerFunction()
        {
            var registry = new EventHandlerRegistryBuilder()
                .Register<SomethingEvent, string>(e => "boo yea")
                .Register<SomethingElseEvent, string>(e => "santa claus")
                .Build();

            var r1 = (string)registry.Raise(new SomethingEvent());
            var r2 = (string)registry.Raise(new SomethingElseEvent());
            r1.Should().Be("boo yea");
            r2.Should().Be("santa claus");
        }

        public record SomethingEvent : DomainEvent;
        public record SomethingElseEvent : DomainEvent;
    }
}