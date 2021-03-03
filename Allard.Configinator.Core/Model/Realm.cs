using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Events;

namespace Allard.Configinator.Core.Model
{
    public class Realm
    {
        private readonly Dictionary<string, Habitat> habitats = new();


        internal OrganizationAggregate Organization { get; }
        public RealmId Id { get; }

        public IReadOnlyCollection<Habitat> Habitats => habitats.Values;

        /// <summary>
        /// Does the add. Used by the event handler.
        /// </summary>
        /// <param name="habitat"></param>
        internal void AddHabitat(Habitat habitat)
        {
            habitats.Add(habitat.Id.Name, habitat);
        }

        public Habitat CreateHabitat(string habitatName, params string[] baseHabitats)
        {
            habitatName = habitatName.NormalizeModelMemberName(nameof(habitatName));
            baseHabitats = baseHabitats.Select(b => b.NormalizeModelMemberName(nameof(baseHabitats))).ToArray();

            // make sure habitat doesn't already exist.
            if (habitats.Values.Any(h => h.Id.Name.Equals(habitatName, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidOperationException("A habitat of that name already exists.");
            }

            var toTest = new HierarchyElement(habitatName, baseHabitats.ToHashSet());
            var existingHabitats = habitats
                .Values
                .Select(h =>
                    new HierarchyElement(h.Id.Name, h.Bases.Select(b => b.Id.Name).ToHashSet()));
            HierarchyValidator.Validate(toTest, existingHabitats);
            var id = new HabitatId(Guid.NewGuid().ToString(), habitatName);
            return Organization.EventHandlerRegistry.Raise<HabitatCreatedEvent, Habitat>(
                new HabitatCreatedEvent(Organization.OrganizationId, Id, id, ));
        }

        public Realm(RealmId id, OrganizationAggregate organization)
        {
            Organization = organization;
            Id = id;
        }
    }

    public record RealmId(string Id, string Name);
}