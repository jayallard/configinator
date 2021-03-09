using System;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class OrganizationAggregateTests
    {
        [Fact]
        public void RealmDoesntExistThrowsException()
        {
            var org = new OrganizationAggregate(OrganizationId.NewOrganizationId("allard"));
            Action test = () => org.GetRealmByName("boom");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Realm doesn't exist. Name=boom");
        }
    }
}