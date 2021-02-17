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


        /// <summary>
        /// Tests that exhaustive.yml is rendered
        /// as expected. compare exhaustive.yml parsed schema
        /// to "exhaustive-results.yml".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ExhaustiveTest()
        {
            var schema = await new SchemaTester(testOutputHelper)
                .Test("exhaustive");
            testOutputHelper.WriteLine("------------------------------------------------------");
            testOutputHelper.WriteLine(schema.ToSampleJson().Single().RootElement.ToString());
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
                .WithMessage("Invalid secret names: xyz");
        }
    }
}