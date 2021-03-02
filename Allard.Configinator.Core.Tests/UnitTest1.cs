using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var id = EventSourcingModel.OrganizationId.NewOrganizationId;
            var org = new EventSourcingModel.Organization(id);
            org.Id.Should().Be(id);
        }
    }
}