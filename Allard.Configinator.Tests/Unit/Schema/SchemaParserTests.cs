using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit.Schema
{
    // TODO: primitives don't resolve against primitive-types.
    // in fact, i think the file could be deleted and nothing 
    // would notice. that'll need to be fixed for parameterized
    // validations. the expected type in the tests will probably
    // change from string to string
    public class Tests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        ///     Schemas/TestTypes/Types contains a bunch of Schema Types.
        ///     Schemas/TestTypes/ExpectedResolution contains yml files that
        ///     describe how each of the types in /Types should resolve.
        ///     This test iterates the types files and parses them.
        ///     It compares the results to the ExpectedResolution files.
        ///     This is an easy way to test the parsing without writing
        ///     a million asserts per file.
        ///     IE:
        ///     Actual = parse(Types/x.yml)
        ///     Expected = ExpectedResolution/xyml
        /// </summary>
        /// <param name="namespaceName"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(GetTestFiles))]
        public async Task Reconcile(string namespaceName)
        {
            await Verify(namespaceName);
        }

        [Fact]
        public void InvalidSecretThrowsException()
        {
            Func<Task> get = async () =>
                await TestUtility.CreateSchemaParser().GetSchemaType("invalid-secret-name/invalid-secret-name");
            get
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Secrets contains invalid property names.\nInvalid: xyz\nValid: user-id, password");
        }


        public static IEnumerable<object[]> GetTestFiles()
        {
            var folder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TestFiles",
                "Schemas",
                "TestTypes",
                "Types");
            return Directory
                .GetFiles(folder, "*.yml")
                .Select(Path.GetFileNameWithoutExtension)
                .Select(f => new object[] {f});
        }

        private async Task Verify(string nameSpace)
        {
            var expectedDoc =
                (await YamlUtility.GetYamlFromFile("TestFiles", "Schemas", "TestTypes", "ExpectedResolution",
                    nameSpace + ".yml"))
                .Single().RootNode;
            var typesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "Schemas", "TestTypes",
                "Types");
            var parser = new SchemaParser(new FileSchemaMetaRepository(typesFolder));
            var expectedTypes = expectedDoc.AsMap("types");
            foreach (var expectedType in expectedTypes)
            {
                var expectedTypeId = expectedType.Key.AsString();
                testOutputHelper.WriteLine("Type: " + expectedTypeId);
                var actualType = await parser.GetSchemaType(expectedTypeId);
                actualType.SchemaTypeId.FullId.Should().Be(expectedTypeId);

                var expectedProperties = expectedType.Value.AsMap("properties");
                VerifyProperties(expectedProperties, actualType.Properties.ToList());
            }
        }

        private void VerifyProperties(YamlMappingNode expectedProperties, List<Property> actualProperties,
            int level = 0)
        {
            var spacing1 = new string(' ', (level + 1) * 3);
            var spacing2 = new string(' ', (level + 2) * 3);
            actualProperties.Count.Should().Be(expectedProperties.Children.Count);
            foreach (var expectedProperty in expectedProperties)
            {
                // verify the property exists
                var expectedName = expectedProperty.Key.AsString();
                testOutputHelper.WriteLine(spacing1 + expectedName);
                var actualProperty = actualProperties.Single(p => p.Name == expectedName);

                // verify the type
                var expectedPropertyType = expectedProperty.Value.AsString("type");
                testOutputHelper.WriteLine(spacing2 + "Type: " + expectedPropertyType);
                actualProperty.TypeId.FullId.Should().Be(expectedPropertyType);

                // nested properties, if there are any
                var expectedNestedProperties = expectedProperty.Value.AsMap("properties");
                var isPrimitive = expectedNestedProperties.Children.Count == 0;
                if (isPrimitive)
                {
                    actualProperty.GetType().Should().Be(typeof(PropertyPrimitive));
                    var actualPrim = (PropertyPrimitive) actualProperty;

                    // verify secret
                    var expectedIsSecret = expectedProperty.Value.AsBoolean("is-secret");
                    testOutputHelper.WriteLine(spacing2 + "Secret: " + expectedIsSecret);
                    actualPrim.IsSecret.Should().Be(expectedIsSecret);
                    
                    // verify optional
                    var expectedIsOptional = expectedProperty.Value.AsBoolean("is-optional");
                    testOutputHelper.WriteLine(spacing2 + "Optional: " + expectedIsOptional);
                    actualPrim.IsOptional.Should().Be(expectedIsOptional);
                    continue;
                }

                actualProperty.GetType().Should().Be(typeof(PropertyGroup));
                VerifyProperties(expectedNestedProperties, ((PropertyGroup) actualProperty).Properties.ToList(),
                    level + 1);
            }
        }
    }
}