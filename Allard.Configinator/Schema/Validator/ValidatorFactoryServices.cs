using Microsoft.Extensions.DependencyInjection;

namespace Allard.Configinator.Schema.Validator
{
    public class ValidatorFactoryServices : ITypeValidatorFactory
    {
        public ITypeValidator GetValidator(SchemaTypeId typeId)
        {
            // todo: register. obviously this shouldn't be
            // hard coded. 
            switch (typeId.FullId)
            {
                case "string":
                    return new StringValidator();
                default:
                    return null;
            }
        }
    }
}