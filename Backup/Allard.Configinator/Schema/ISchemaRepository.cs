using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public interface ISchemaRepository
    {
        Task<YamlMappingNode> GetSchema(string id);
    }
}