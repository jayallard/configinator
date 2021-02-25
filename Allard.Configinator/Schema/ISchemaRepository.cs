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
        Task<IEnumerable<ModelDto.TypeDto>> GetSchemaTypes();
    }
}