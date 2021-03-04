using System.Collections.Generic;

namespace Allard.Configinator.Core.Tests.Unit
{
    public class OrganizationDto
    {
        public string Name { get; set; }
        public List<RealmDto> Realms { get; set; }
        public List<SchemaTypeDto> SchemaTypes { get; set; }
    }

    public class SchemaTypeDto
    {
        public string SchemaTypeId { get; set; }
        public List<PropertyGroupDto> PropertyGroups { get; set; }
        public List<PropertyDto> Properties { get; set; }
    }

    public class PropertyDto
    {
        public string Name { get; set; }
        public string SchemaTypeId { get; set; }
    }

    public class PropertyGroupDto
    {
        public string Name { get; set; }
        public string SchemaTypeId { get; set; }
        public List<PropertyGroupDto> PropertyGroups { get; set; }
        public List<PropertyDto> Properties { get; set; }
    }

    public class RealmDto
    {
        public string Name { get; set; }
        public List<ConfigurationSectionDto> ConfigurationSections { get; set; }
        public List<HabitatDto> Habitats { get; set; }
    }

    public class HabitatDto
    {
        public string Name { get; set; }
        public IList<string> Bases { get; set; }
    }
    
    public class ConfigurationSectionDto
    {
        public string Name { get; set; }
        public List<PropertyGroupDto> PropertyGroups { get; set; }
        public List<PropertyDto> Properties { get; set; }
    }
    
    // todo: can't assign primitive type to configuration section.
}