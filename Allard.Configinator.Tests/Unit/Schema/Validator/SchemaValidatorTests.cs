using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Schema;
using Allard.Configinator.Schema.Validator;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Tests.Unit.Schema.Validator
{
    public class SchemaValidatorTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SchemaValidatorTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Schema defines 3 properties in the object.
        /// The json is missing 1.
        /// </summary>
        [Fact]
        public void RootPrimitiveProperties()
        {
            var factory = new ValidatorFactoryServices();
            var validator = new SchemaValidator(factory);
            var typeId = new SchemaTypeId("test/test");
            var stringId = new SchemaTypeId("string");
            var properties = new List<Property>
                {
                    new PropertyPrimitive("Prop1", stringId, false),
                    new PropertyPrimitive("Prop2", stringId, false),
                    new PropertyPrimitive("Prop3", stringId, false),
                }
                .AsReadOnly();

            var type = new SchemaParser.ObjectSchemaType(typeId, properties);
            var json = JToken.Parse("{ \"Prop2\": \"b\", \"Prop3\": \"c\" }");
            
            var results = validator.Validate(json, type);
            results.Count.Should().Be(1);
            results.First().Should()
                .Be(new TypeValidationError("Core", "/@Prop1", "Required property doesn't exist: Prop1"));
            testOutputHelper.WriteLine(results.Single().ToString());
        }
    }
}