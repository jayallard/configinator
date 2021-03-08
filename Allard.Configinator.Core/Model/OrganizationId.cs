using System;

namespace Allard.Configinator.Core.Model
{
    public record OrganizationId(string Name, string Id)
    {
        public static OrganizationId NewOrganizationId(string name)
        {
            return new(name, Guid.NewGuid().ToString());
        }
    }
}