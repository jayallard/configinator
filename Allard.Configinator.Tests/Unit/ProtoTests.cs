using Xunit;

namespace Allard.Configinator.Tests.Unit
{
    public class ProtoTests
    {
        [Fact]
        public void Proto()
        {
            var configService = new ConfgurationService();
            var namespaces = configService.GetNamespaces();
            
            var ns = configService.GetNamespaces(namespaces.First().Id);
            
            
        }
    }
}