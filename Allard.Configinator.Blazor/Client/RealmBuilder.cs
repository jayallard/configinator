using System.Collections.Generic;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;

namespace Allard.Configinator.Blazor.Client
{
    public class RealmBuilder
    {
        public List<Habitat> Habitats { get; } = new();
        public List<ConfigurationSection> ConfigurationSections = new();
        public string RealmId { get; set; }
        public string OrganizationId { get; set; }
    }

    public class ConfigurationSection
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<PropertyViewModel> Properties { get; } = new();
    }
    
    public class Habitat
    {
        public string Name { get; set; }
        public List<string> Bases { get; } = new();
    }
}