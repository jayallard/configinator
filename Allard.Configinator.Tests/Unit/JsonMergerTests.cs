using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Tests.Unit
{
    public class JsonMergerTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public JsonMergerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SkipNullDocuments()
        {
            var doc1 = JToken.Parse("{ \"hello\": \"world\", \"a\": \"b\" }");
            var doc3 = JToken.Parse("{ \"hello\": \"planet\" }");
            var expected = JToken.Parse("{ \"hello\": \"planet\", \"a\": \"b\" }");
            
            var result = new JsonMerger(doc1, null, doc3).Merge();
            JToken.DeepEquals(result, expected).Should().BeTrue();
        }

        /// <summary>
        ///     If there's only one document, then there's nothing
        ///     to merge. so, it returns the only doc it received..
        /// </summary>
        [Fact]
        public void OneDocumentReturnsItself()
        {
            var doc = JToken.Parse("{ \"hello\": \"world\" }");
            var merged = new JsonMerger(doc).Merge();
            merged.Should().BeEquivalentTo(doc);
        }

        [Fact]
        public void ScalarNumber()
        {
            var doc = MergeFromStrings("9", "10", "11");
            doc.Value<int>().Should().Be(9);
        }

        [Fact]
        public void ScalarString()
        {
            var doc = MergeFromStrings("\"a\"", "\"b\"", "\"c\"", "\"d\"");
            doc.Value<string>().Should().Be("a");
        }

        [Fact]
        public void ScalarBoolean()
        {
            MergeFromStrings("true", "false", "true").Value<bool>().Should().BeTrue();
            MergeFromStrings("false", "false", "true").Value<bool>().Should().BeFalse();
        }

        [Fact]
        public void SingleLevelObject()
        {
            var obj = (JObject) MergeFromStrings(
                "{ \"hello\" : \"world\", \"a\": \"b\" }",
                "{ \"hello\" : \"mars\" }");
            obj.Properties().Count().Should().Be(2);
            obj["hello"].Value<string>().Should().Be("world");
            obj["a"].Value<string>().Should().Be("b");
        }

        private static JToken MergeFromStrings(params string[] json)
        {
            return new JsonMerger(json.Select(JToken.Parse)).Merge();
        }

        [Theory]
        [MemberData(nameof(GetMergeTests))]
        public void Merge(MergeData testData)
        {
            var tests = GetMergeTests().ToList();
            testOutputHelper.WriteLine(tests.Count().ToString());

            var merged = new JsonMerger(testData.Input).Merge();
            if (JToken.DeepEquals(merged, testData.ExpectedOutput)) return;

            testOutputHelper.WriteLine("expected ---------");
            testOutputHelper.WriteLine(testData.ExpectedOutput.ToString());
            testOutputHelper.WriteLine("actual output ---------");
            testOutputHelper.WriteLine(merged.ToString());
            Assert.True(false);
        }

        public static IEnumerable<object[]> GetMergeTests()
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "JsonMerge");
            return Directory.GetFiles(folder, "*.json")
                .Select(f =>
                {
                    var parts = Path.GetFileNameWithoutExtension(f).Split(".");
                    var testName = parts[0];
                    var isResult = parts[1] == "result";
                    var partNumber = isResult
                        ? 0
                        : int.Parse(parts[1]);
                    return new
                    {
                        TestName = testName,
                        IsResult = isResult,
                        PartNumber = partNumber,
                        FileName = f
                    };
                })
                .GroupBy(g => g.TestName)
                .Select(g =>
                {
                    var output = g.Single(i => i.IsResult);
                    var input = g
                        .Where(i => !i.IsResult)
                        .OrderBy(i => i.PartNumber)
                        .Select(i => JsonUtility.GetFile(i.FileName))
                        .ToList();
                    return new MergeData(g.Key, input, JsonUtility.GetFile(output.FileName));
                })
                .Select(test => new[] {test});
        }
    }

    [DebuggerDisplay("{TestName}")]
    public record MergeData(string TestName, List<JObject> Input, JObject ExpectedOutput);
}