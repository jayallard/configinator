using System.Collections.Generic;
using Allard.Configinator.Schema;
using Allard.Configinator.Schema.Validator;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Allard.Configinator.Tests.Unit.Schema.Validator
{
    public class SchemaValidatorTests
    {
        [Fact]
        public void Blah()
        {
            var factory = new ValidatorFactoryServices();
            var validator = new SchemaValidator(factory);
            var typeId = new SchemaTypeId("test/test");
            var stringId = new SchemaTypeId("string");
            var properties = new List<Property>
                {
                    new PropertyPrimitive("Prop1", stringId, false)
                }
                .AsReadOnly();

            var type = new SchemaParser.ObjectSchemaType(typeId, properties);
            var json = JToken.Parse("{}");
            var results = validator.Validate(json, type);
        }
    }
}