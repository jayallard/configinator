using System.Collections.Generic;
using System.Collections.ObjectModel;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.Builders;
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
            /*
             *      types
             *          a/b
             */
            
            var idB = SchemaTypeId.Parse("x/y");
            var idC = SchemaTypeId.Parse("santa/claus");

            var emptySchemaTypeList = new List<SchemaType>().AsReadOnly();
            var groupB = PropertyGroupBuilder
                .Create("p2", idC)
                .AddProperty("name", SchemaTypeId.String.FullId)
                .Build();
            
            // object a:
            //      name = p1
            //      type = a/b
            // it contains a nested object
            var groupA = PropertyGroupBuilder
                .Create("p1", idB)
                .AddPropertyGroup(groupB)
                .Build();

            // todo: shouldn't have to do this. primitives aren't done.
            var schemaTypeA = SchemaTypeBuilder
                .Create(SchemaTypeId.String.FullId)
                .AddProperty("name", SchemaTypeId.String.FullId)
                .AddPropertyGroup(groupA)
                .Build();

            var schemaTypeB = SchemaTypeBuilder
                .Create("x/y")
                .AddProperty("name", SchemaTypeId.String)
                .Build();

            var schemaTypeC = SchemaTypeBuilder
                .Create("santa/claus")
                .Build();
            
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