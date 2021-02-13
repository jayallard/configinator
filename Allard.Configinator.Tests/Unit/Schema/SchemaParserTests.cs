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

        [Fact]
        public async Task DoEverythingBetter()
        {
            await new SchemaTester(testOutputHelper)
                .Test("exhaustive");
        }
        
    }
}