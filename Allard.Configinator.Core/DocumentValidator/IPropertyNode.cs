namespace Allard.Configinator.Core.DocumentValidator
{
    public interface IPropertyNode
    {
        string Name { get; }
        object Value { get; }
    }
}