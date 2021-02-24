using System.Threading.Tasks;

namespace Allard.Configinator.Schema
{
    public interface ISchemaService
    {
        Task<ObjectSchemaType> GetSchemaTypeAsync(string typeId);
    }
}