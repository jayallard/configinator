using System;

namespace Allard.Configinator.Core.Model
{
    public record RealmId(string Id, string Name) : ModelMemberId(Id, Name)
    {
        public static RealmId NewRealmId(string realmName)
        {
            return new(Guid.NewGuid().ToString(), realmName);
        }
    }
}