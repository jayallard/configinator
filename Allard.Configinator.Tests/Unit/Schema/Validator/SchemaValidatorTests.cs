using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using Allard.Configinator.Schema.Validator;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Allard.Configinator.Tests.Unit.Schema.Validator
{
    public class SchemaValidatorTests
    {
        private static readonly string Folder = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "TestFiles",
            "Validator");

        private readonly ISchemaService service;

        public SchemaValidatorTests()
        {
            ISchemaRepository schemaRepository = new SchemaRepositoryYamlFiles(Folder);
            service = new SchemaService(schemaRepository, new SchemaValidator(new ValidatorFactoryServices()));
        }

        [Theory]
        [MemberData(nameof(GetTests))]
        public async Task Validate(
            ObjectSchemaType type,
            JToken json,
            List<TypeValidationError> expectedResponses)
        {
            var factory = new ValidatorFactoryServices();
            var validator = new SchemaValidator(factory);
            var results = await validator.Validate(json, type);
            results.Count.Should().Be(expectedResponses.Count);
            for (var i = 0; i < results.Count; i++) results[i].Should().Be(expectedResponses[i]);
        }

        public static IEnumerable<object[]> GetTests()
        {
            var schemaRepo = new SchemaRepositoryYamlFiles(Folder);
            var schemaParser = new SchemaService(schemaRepo, new SchemaValidator(new ValidatorFactoryServices()));
            return Directory
                .GetFiles(Folder, "*.json")
                .SelectMany(fileName =>
                {
                    var json = JsonUtility.GetFile(fileName);
                    var type = (string) json["type"];
                    return json["tests"]
                        .ToArray()
                        .Select(t =>
                        {
                            return new object[]
                            {
                                // blocking... hack.
                                schemaParser.GetSchemaTypeAsync(type).Result,
                                t["test-value"],
                                t["expected-failures"]
                                    .ToArray()
                                    .Select(e =>
                                    {
                                        var parts = ((string) e).Split("||");
                                        return new TypeValidationError(
                                            parts[0].Trim(),
                                            parts[1].Trim(),
                                            parts[2].Trim());
                                    }).ToList()
                            };
                        });
                });
        }
    }
}