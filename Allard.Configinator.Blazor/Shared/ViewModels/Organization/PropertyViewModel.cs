namespace Allard.Configinator.Blazor.Shared.ViewModels.Organization
{
    public class PropertyViewModel
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSecret { get; set; }
        public string SchemaTypeId { get; set; }
    }
}