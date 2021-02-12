using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public interface ISchemaRepository
    {
        Task<YamlNode> GetSchema(string id);
    }
}