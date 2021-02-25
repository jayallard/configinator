using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allard.Configinator.Schema
{
    /// <summary>
    ///     Convert schema DTOs to actual scheme objects.
    ///     The DTOs contain basic information about each individual type,
    ///     as loaded from a file or other source.
    ///     This does the work of putting creating full-formed schemas.
    /// </summary>
    public class SchemaResolver
    {
        /// <summary>
        ///     The DTOs.
        /// </summary>
        private readonly Dictionary<SchemaTypeId, ModelDto.TypeDto> inputByTypeId;

        /// <summary>
        ///     The schema types.
        /// </summary>
        private readonly Dictionary<SchemaTypeId, ObjectSchemaType> outputByTypeId = new();

        private SchemaResolver(IEnumerable<ModelDto.TypeDto> dto)
        {
            inputByTypeId = dto.ToDictionary(d => new SchemaTypeId(d.Namespace + "/" + d.TypeName));
        }
        // TODO: prevent ciruclar references.

        public static async Task<IEnumerable<ObjectSchemaType>> ConvertAsync(IEnumerable<ModelDto.TypeDto> dto)
        {
            return await new SchemaResolver(dto).ConvertAsync();
        }

        public async Task<IEnumerable<ObjectSchemaType>> ConvertAsync()
        {
            foreach (var (id, dto) in inputByTypeId)
            {
                if (IsResolved(id))
                    // already done. move along.
                    continue;

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
            if (outputByTypeId.ContainsKey(id)) return outputByTypeId[id];

            var dto = inputByTypeId[id];
            var type = await BuildResolvedType(dto);
            outputByTypeId[id] = type;
            return type;
        }

        private async Task<ObjectSchemaType> BuildResolvedType(ModelDto.TypeDto dto)
        {
            var typeId = new SchemaTypeId(dto.Namespace + "/" + dto.TypeName);
            var properties = new List<Property>();

            // get the properties from the base type.
            if (dto.BaseTypeName != null)
            {
                var baseId = NormalizeTypeId(dto.Namespace, dto.BaseTypeName);
                var baseType = await GetResolvedType(baseId);
                properties.AddRange(baseType.Properties);
            }

            // get the properties for this type.
            properties.AddRange(await GetProperties(dto.Namespace, dto));
            return new ObjectSchemaType(typeId, properties.AsReadOnly());
        }

        private async Task<List<Property>> GetProperties(string relativeNamespace, ModelDto.TypeDto dto)
        {
            var results = new List<Property>();
            foreach (var property in dto.Properties)
            {
                var propertyTypeId = NormalizeTypeId(relativeNamespace, property.TypeName);
                var isOptional = property.IsOptional || dto.Optional.Contains(property.PropertyName);
                var isSecret = property.IsSecret || dto.Secrets.Contains(property.PropertyName);
                if (propertyTypeId.IsPrimitive)
                {
                    results.Add(new PropertyPrimitive(property.PropertyName,
                        propertyTypeId,
                        isSecret,
                        isOptional));
                    continue;
                }

                var childType = await GetResolvedType(propertyTypeId);
                results.Add(new PropertyGroup(property.PropertyName, propertyTypeId, isOptional,
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