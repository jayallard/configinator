namespace Allard.Configinator.Core.DocumentValidator
{
    /// <summary>
    ///     Property value of an IObjectNode.
    ///     Name/Value pair, so this is general use.
    /// </summary>
    public record PropertyNode(string Name, object Value) : IPropertyNode;
}