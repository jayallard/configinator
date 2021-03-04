using System.Collections.Generic;
using System.Collections.ObjectModel;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Validators;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Validators
{
    public class SchemaTypeValidatorTests
    {
        [Fact]
        public void Redo()
        {
        }

        [Fact]
        public void Test1()
        {
            var idA = SchemaTypeId.Parse("a/b");
            var idB = SchemaTypeId.Parse("x/y");
            var idC = SchemaTypeId.Parse("santa/claus");

            var emptyProperties = new List<Property>().AsReadOnly();
            var emptyPropertyGroups = new List<PropertyGroup>().AsReadOnly();
            var emptySchemaTypeList = new List<SchemaType>().AsReadOnly();

            var groupB = new PropertyGroup("p2", idC, false, new List<Property>
            {
                new("name", SchemaTypeId.String)
            }.AsReadOnly(), emptyPropertyGroups);
            var groupA = new PropertyGroup("p1", idB, false, emptyProperties, ToReadonly(groupB));

            var schemaTypeA = new SchemaType(idA, new List<Property>
            {
                new("name", SchemaTypeId.String)
            }.AsReadOnly(), ToReadonly(groupA));

            var schemaTypeB = new SchemaType(SchemaTypeId.Parse("x/y"), new List<Property>
            {
                new("name", SchemaTypeId.String)
            }.AsReadOnly(), emptyPropertyGroups);
            var schemaTypeC = new SchemaType(SchemaTypeId.Parse("santa/claus"), emptyProperties, emptyPropertyGroups);

            new SchemaTypeValidator(
                    schemaTypeB,
                    emptySchemaTypeList)
                .Validate();

            new SchemaTypeValidator(
                    schemaTypeA,
                    ToReadonly(schemaTypeB, schemaTypeC))
                .Validate();
        }

        private static ReadOnlyCollection<T> ToReadonly<T>(params T[] values)
        {
            return new List<T>(values).AsReadOnly();
        }
    }
}