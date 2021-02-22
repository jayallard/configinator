namespace Allard.Configinator.Schema.Validator
{
    public interface ITypeValidatorFactory
    {
        ITypeValidator GetValidator(SchemaTypeId tyeId);
    }
}