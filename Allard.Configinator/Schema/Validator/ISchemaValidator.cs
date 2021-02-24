using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Schema.Validator
{
    public interface ISchemaValidator
    {
        Task<IList<TypeValidationError>> Validate(JToken document, ObjectSchemaType type);
    }
}