using System;
using System.IO;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using Allard.Configinator.Schema.Validator;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Tests.Unit.Schema
{
    public class SchemaServiceTests
    {
        private readonly string baseFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "FullSetup");

        [Fact]
        public void ThrowExceptionIfSchemaTypeDoesntExist()
        {
            var schemaService = new SchemaService(new SchemaRepositoryYamlFiles(baseFolder),
                new SchemaValidator(new ValidatorFactoryServices()));
            Func<Task> boom = async () => await schemaService.GetSchemaTypeAsync("go boom");
            boom.Should().Throw<SchemaNotFoundException>();
        }
    }
}