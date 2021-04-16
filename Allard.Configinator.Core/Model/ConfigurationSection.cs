using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Allard.Configinator.Core.Model
{
    public class ConfigurationSection
    {
        public Realm Realm { get; }
        public SectionId SectionId { get; }
        public IReadOnlyCollection<SchemaTypePropertyExploded> Properties { get; }
        public string Description { get; }

        private readonly Dictionary<string, SchemaTypePropertyExploded> propertyMap;

        public ConfigurationSection(Realm realm, SectionId sectionId,
            IEnumerable<SchemaTypePropertyExploded> properties, string description)
        {
            Realm = realm;
            SectionId = sectionId;
            propertyMap = properties.ToDictionary(p => p.Name);
            Properties = propertyMap.Values;
            Description = description;
        }

        public SchemaTypePropertyExploded Find(string path)
        {
            path.EnsureValue(nameof(path));
            var parts = path.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var current = GetProperty(parts[0]);
            for (var i = 1; i < parts.Length; i++)
            {
                current = current.GetProperty(parts[i]);
            }

            return current;
        }

        public bool PathExists(string path)
        {
            path.EnsureValue(nameof(path));
            var parts = path.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (!PropertyExists(parts[0]))
            {
                return false;
            }

            var current = GetProperty(parts[0]);
            for (var i = 1; i < parts.Length; i++)
            {
                if (!current.PropertyExists(parts[i]))
                {
                    return false;
                }

                current = current.GetProperty(parts[i]);
            }

            return true;
        }

        public SchemaTypePropertyExploded GetProperty(string propertyName)
        {
            return propertyMap[propertyName];
        }

        public bool PropertyExists(string propertyName)
        {
            return propertyMap.ContainsKey(propertyName);
        }
    }
}