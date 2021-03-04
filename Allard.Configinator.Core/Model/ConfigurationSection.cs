using System;

namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSection(ConfigurationSectionId ConfigurationSectionId, string Path, SchemaType SchemaType,
        string Description);
    

 
    
    public record ConfigurationSectionId(string Id, string Name) : ModelMemberId(Id, Name)
    {
        public static ConfigurationSectionId NewConfigurationSectionId(string name)
        {
            return new (Guid.NewGuid().ToString(), name);
        }
    }
}
