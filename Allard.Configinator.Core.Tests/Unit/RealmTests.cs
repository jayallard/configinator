using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class RealmTests
    {
        [Fact]
        public void BaseRealmMustExist()
        {
            var org = new OrganizationAggregate(new OrganizationId("blah"));
            var realm = org.AddRealm("abc");
            var test = new Action(() => realm.AddHabitat("new", "boom"));
            test
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("BaseHabitat doesn't exist: boom");
        }
        
        [Fact]
        public void RealmIdMustBeUnique()
        {
            var org = new OrganizationAggregate(new OrganizationId("blah"));
            var realm = org.AddRealm("abc");
            realm.AddHabitat("abc");
            var test = new Action(() => realm.AddHabitat("abc"));
            test
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("HabitatId already exists. Id=abc");
        }
        
        [Fact]
        public void AddDuplicateRealmFails()
        {
            var orgId = new OrganizationId("allard");
            var org = new OrganizationAggregate(orgId);

            org.Realms.Should().BeEmpty();
            org.AddRealm("ALLARD-REALM-1");
            Action test = () => org.AddRealm("ALLARD-REALM-1");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("RealmId already exists. Id=allard-realm-1");
        }
        
        [Fact]
        public async Task AddConfigurationSectionFailsIfSchemaTypeDoesntExist()
        {
            var orgId = new OrganizationId("allard");
            var org = new OrganizationAggregate(orgId);
            var realm = org.AddRealm("blah");
            var properties = new List<SchemaTypeProperty>
            {
                new("name", SchemaTypeId.Parse("a/b"))
            };
            Action test = () => realm.AddConfigurationSection("cs", properties, "description");
            test.Should().Throw<InvalidOperationException>()
                .WithMessage("The SchemaTypeIds don't exist in the organization: a/b");
        }
    }
}