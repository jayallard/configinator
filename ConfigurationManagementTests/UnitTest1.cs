using System.Threading.Tasks;
using ConfigurationManagement.Code.Schema;
using Xunit;
using Xunit.Abstractions;

namespace ConfigurationManagementTests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Test1()
        {
            const string schemaFolder =
                @"/Users/jallard/personal/ConfigurationManagement/ConfigurationManagement/Schemas";
            var repo = new FileSchemaRepository(schemaFolder);
            var parser = new SchemaParser2(repo);
            var schema = await parser.GetSchema("sample1");
            output.WriteLine("yay");
        }
    }
}