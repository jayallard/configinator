using System;

namespace Allard.Configinator.Core.Model
{
    public record OrganizationId(string Id)
    {
        public static OrganizationId NewOrganizationId()
        {
            return new(Guid.NewGuid().ToString());
        }
    }
}