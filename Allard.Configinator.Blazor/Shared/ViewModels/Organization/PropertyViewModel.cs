namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class PropertyViewModel
    {
        public string Name { get; init; }
        public bool IsRequired { get; init; }
        public bool IsSecret { get; set; }
        public string SchemaTypeId { get; init; }
    }
}