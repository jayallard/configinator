using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core
{
    public class Test
    {
        public void Blah()
        {
             var org = new Organization("id", "allard");
             var realm = org.CreateRealm("TestRealm1");
        }
    }
    
    public class Organization
    {
        private Dictionary<string, Realm> realms = new();
        private Dictionary<string, SchemaType> schemaTypes = new();
        private Dictionary<string, Habitat> habitats = new();
        
        public Organization(string organizationId, string organizationName)
        {
            OrganizationId = organizationId;
            OrganizationName = organizationName;
        }
        
        public string OrganizationId { get; }
        public string OrganizationName { get; }

        public IReadOnlyCollection<Realm> Realms => realms.Values;
        public IReadOnlyCollection<SchemaType> SchemaTypes => schemaTypes.Values;
        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;

        public Realm CreateRealm(string name)
        {
            // name required
            name = name.Trim();
            if (Realms.Any(r => string.Equals(r.RealmName, name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new Exception("realm already exists");
            }

            var realm = new Realm(name, this);
            return realm;
        }
    }

    public class Realm
    {
        private readonly Organization org;
        internal Realm(string name, Organization org)
        {
            RealmName = name;
            this.org = org;
        }
        public string RealmName { get; }
    }

    public class ConfigurationSection
    {
        public string ConfigurationSectionName { get; }
        public string Path { get; }
    }

    public record SchemaTypeId(string Namespace, string Name);
    
    public class Property
    {
        public SchemaType SchemaType { get; }
        public string PropertyName { get; }
        public bool IsRequired { get; }
        public bool IsSecret { get; }
        public IReadOnlyCollection<Property> Properties { get; }
    }
    
    public class Habitat
    {
        public string Name { get; }
    }

    public class SchemaType
    {
        public string Name { get; }
    }
    
    public static class ModelExtensionMethods{
    }
}