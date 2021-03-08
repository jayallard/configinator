using System;

namespace Allard.Configinator.Core.Model
{
    public record HabitatId(string Id, string Name) : ModelMemberId(Id, Name)
    {
        public static HabitatId NewHabitatId(string name)
        {
            return new(Guid.NewGuid().ToString(), name);
        }
    }
}