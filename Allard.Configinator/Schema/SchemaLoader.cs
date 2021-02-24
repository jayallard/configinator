using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allard.Configinator.Schema
{
    public class SchemaLoader
    {
        public static async Task<IEnumerable<ObjectSchemaType>> Convert(IEnumerable<ModelDto.TypeDto> dto)
        {
            return await new SchemaLoader(dto).Convert();
        }

        private readonly Dictionary<SchemaTypeId, ModelDto.TypeDto> inputByTypeId;
        private readonly Dictionary<SchemaTypeId, ObjectSchemaType> outputByTypeId;

        private SchemaLoader(IEnumerable<ModelDto.TypeDto> dto)
        {
            inputByTypeId = dto.ToDictionary(d => new SchemaTypeId(d.TypeName));
        }

        public async Task<IEnumerable<ObjectSchemaType>> Convert()
        {
            foreach (var (id, dto) in inputByTypeId)
            {
                if (IsResolved(id))
                {
                    // already done. move along.
                    continue;
                }

                outputByTypeId[id] = await BuildResolvedType(dto);
            }

            return outputByTypeId.Values;
        }

        private bool IsResolved(SchemaTypeId id)
        {
            return outputByTypeId.ContainsKey(id);
        }
        
        private async Task<ObjectSchemaType> GetResolvedType(SchemaTypeId id)
        {
            if (outputByTypeId.ContainsKey(id))
            {
                return outputByTypeId[id];
            }

            var dto = inputByTypeId[id];
            var type = await BuildResolvedType(dto);
            outputByTypeId[id] = type;
            return type;
        }

        private async Task<ObjectSchemaType> BuildResolvedType(ModelDto.TypeDto dto)
        {
            var typeId = new SchemaTypeId(dto.TypeName);
            var properties = await GetProperties(dto.Namespace, dto);
            return new ObjectSchemaType(typeId, properties.AsReadOnly());
        }

        private async Task<List<Property>> GetProperties(string relativeNamespace, ModelDto.TypeDto dto)
        {
            var results = new List<Property>();
            foreach (var property in dto.Properties)
            {
                var propertyTypeId = NormalizeTypeId(relativeNamespace, property.TypeName);
                if (propertyTypeId.IsPrimitive)
                {
                    results.Add(new PropertyPrimitive(property.PropertyName, propertyTypeId, property.IsSecret,
                        property.IsOptional));
                    continue;
                }

                var childType = await GetResolvedType(propertyTypeId);
                results.Add(new PropertyGroup(property.PropertyName, propertyTypeId, property.IsOptional,
                    childType.Properties));
            }

            return results;
        }

        /// <summary>
        ///     Converts a type id to a standard format.
        ///     If the id is "./type", it is converted to "relativeSchemaId/type".
        ///     If the id starts with "/x", it is converted to "primitive-types/x",
        ///     where x can be any primitive type.
        /// </summary>
        /// <param name="relativeNamespace"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        private static SchemaTypeId NormalizeTypeId(string relativeNamespace, string typeId)
        {
            return typeId.StartsWith("./")
                // if references this schema, then change the ./ to this schema id.
                // IE:  "./typeId" becomes "currentSchemaId/typeId"
                ? new SchemaTypeId(relativeNamespace + "/" + typeId.Substring(2))
                : new SchemaTypeId(typeId);
        }
    }
}