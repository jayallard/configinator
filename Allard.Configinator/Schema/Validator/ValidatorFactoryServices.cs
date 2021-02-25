namespace Allard.Configinator.Schema.Validator
{
    public class ValidatorFactoryServices : ITypeValidatorFactory
    {
        public ITypeValidator GetValidator(SchemaTypeId typeId)
        {
            // todo: register. obviously this shouldn't be
            // hard coded. 
            return typeId.FullId switch
            {
                "string" => new StringValidator(),
                _ => null
            };
        }
    }
}