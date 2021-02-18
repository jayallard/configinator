using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public interface ISchemaRepository
    {
        /// <summary>
        /// Retrieve the raw yaml of a schema.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<YamlMappingNode> GetSchemaYaml(string id);
    }
}