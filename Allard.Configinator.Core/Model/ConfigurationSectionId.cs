using System;

namespace Allard.Configinator.Core.Model
{
    public record ConfigurationSectionId(string Id, string Name) : ModelMemberId(Id, Name)
    {
        public static ConfigurationSectionId NewConfigurationSectionId(string name)
        {
            return new (Guid.NewGuid().ToString(), name);
        }
    }
}