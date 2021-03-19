using System;
using System.Collections.Generic;
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
            var org = new OrganizationAggregate(new OrganizationId("allard"));
            Action test = () => org.GetRealmByName("boom");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("Realm doesn't exist. Name=boom");
        }
    }
}