using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Schema;
using FluentAssertions;
using Microsoft.VisualBasic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit.Schema
{
    public class SchemaParserTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SchemaParserTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public void Test()
        {
            /*
             * var schemas = await blah.GetSchemas();
             *      id, description
             * var schema = await blah.GetSchema(id);
             * 
             */
        }

        /// <summary>
        /// Tests that exhaustive.yml is rendered
        /// as expected. compare exhaustive.yml parsed schema
        /// to "exhaustive-results.yml".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ExhaustiveTest()
        {
            await TestSchema("exhaustive");
        }

        [Fact]
        public async Task InheritAll()
        {
            await TestSchema("inherit-all");
        }

        private async Task TestSchema(string schemaName)
        {
            var schema = await new SchemaTester(testOutputHelper)
                .Test(schemaName);
            foreach (var sample in schema.ToSampleJson())
            {
                testOutputHelper.WriteLine("------------------------------------------------------");
                testOutputHelper.WriteLine(sample.RootElement.ToString());
            }
        }

        /// <summary>
        /// The secrets array can only refer to valid property names.
        /// IE: "secrets": ["a", "b", "c"]
        /// a,b,c must be properties.
        /// </summary>
        [Fact]
        public void SecretNamesMustBeValid()
        {
            // arrange
            Func<Task> action = async () => await TestUtility.CreateSchemaParser().GetSchema("invalid-secret-name");

            // act 
            // assert
            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Secrets contains invalid property names.\nInvalid: xyz\nValid: user-id,password,xyz");
        }
    }
}