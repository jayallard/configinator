using System.Collections.Generic;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     Manage the storage of schema metadata.
    /// </summary>
    public interface ISchemaRepository
    {
        /// <summary>
        ///     Retrieve the raw yaml of a schema.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        Task<YamlMappingNode> GetSchemaYaml(string nameSpace);

        Task<IEnumerable<ModelDto.TypeDto>> GetTypes(string nameSpace);
        Task<IEnumerable<ModelDto.TypeDto>> GetSchemaTypes();
        
    }
}