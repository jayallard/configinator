using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests
{
    public class Junk
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Junk(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Blah()
        {
        }
    }
}