using System.Threading.Tasks;
using Allard.Configinator.Infrastructure.MongoDb;
using Xunit;

namespace Allard.Configinator.Core.Tests
{
    public class Tools
    {
        [Fact]
        public async Task SetupDatabase()
        {
            await new OrganizationRepositoryMongo().DevelopmentSetup();
        }
    }
}