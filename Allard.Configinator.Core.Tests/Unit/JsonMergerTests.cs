using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class JsonMergerTests
    {
        [Fact]
        public void Merge()
        {
            var top = JsonDocument.Parse("{ \"test\": \"hi\", \"obj\": { \"hello\": \"world\" } }").RootElement;
            var middle = JsonDocument.Parse("{ \"santa\": \"claus\", \"x\": \"y\" }").RootElement;
            var bottom = JsonDocument.Parse("{ \"santa\": \"claus\", \"x\": null, \"obj\": { \"hello\": \"moon\", \"a\": \"b\" } }")
                .RootElement;
            
            // flatten might be easier, but UTF8 is better... 
        }
        /*
         * test = hi
         * obj.hello=world
         *
         * santa=claus
         * x=y
         *
         * santa=claus
         * x=null
         * obj.hello=moon
         * obj.a=b
         */
    }
}