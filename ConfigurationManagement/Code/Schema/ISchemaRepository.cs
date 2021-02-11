using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ConfigurationManagement.Code.Schema
{
    public interface ISchemaRepository
    {
        Task<YamlNode> GetSchema(string id);
    }
}