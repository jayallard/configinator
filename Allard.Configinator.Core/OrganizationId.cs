using System;

namespace Allard.Configinator.Core
{
    public record OrganizationId(string Id)
    {
        public static OrganizationId NewOrganizationId => new (Guid.NewGuid().ToString());
    }
}