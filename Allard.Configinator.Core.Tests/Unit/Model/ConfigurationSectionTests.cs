using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.Model
{
    public class ConfigurationSectionTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ConfigurationSectionTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Blah()
        {
            var bogusType = SchemaTypeBuilder
                .Create("bogus/stuff")
                .AddStringProperty("a")
                .AddStringProperty("b")
                .AddStringProperty("c")
                .AddStringProperty("d")
                .Build();

            var kafkaType = SchemaTypeBuilder
                .Create("kafka/unsecured")
                .AddStringProperty("broker-list")
                .Build();

            var sqlType = SchemaTypeBuilder
                .Create("mssql/sql-user")
                .AddStringProperty("host")
                .AddStringProperty("user-id")
                .AddStringProperty("password", true)
                .AddStringProperty("instance", isOptional: true)
                .AddStringProperty("initial-catalog", isOptional: true)
                .AddProperty("bogus1", "bogus/stuff")
                .AddProperty("bogus2", "bogus/stuff")
                .Build();

            var shovelServiceType = SchemaTypeBuilder
                .Create("something-domain/shovel-service")
                .AddProperty("sql-source", "mssql/sql-user")
                .AddProperty("kafka-target", "kafka/unsecured")
                .Build();

            var org = new OrganizationAggregate(new OrganizationId("allard"));
            org.AddSchemaType(bogusType);
            org.AddSchemaType(kafkaType);
            org.AddSchemaType(sqlType);
            org.AddSchemaType(shovelServiceType);

            var realm = org.AddRealm("domain-a");
            realm.AddHabitat("production");
            realm.AddHabitat("staging");
            realm.AddHabitat("dev");
            realm.AddHabitat("dev-allard", "dev");
            var cs = realm.AddConfigurationSection("shovel-service", "something-domain/shovel-service",
                "/{{habitat}}/something-domain/shovel-service", "description");

            var x = new JsonStructureModelBuilder(org.SchemaTypes).ToStructureModel(cs);
            testOutputHelper.WriteLine("");
        }
    }
}