using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public class VersionTracker
    {
        private static readonly IReadOnlyCollection<VersionedObject> EmptyObjects =
            new List<VersionedObject>().AsReadOnly();

        private static readonly IReadOnlyCollection<VersionedProperty> EmptyProperties =
            new List<VersionedProperty>().AsReadOnly();

        private readonly ObjectDto model;
        private readonly Dictionary<string, VersionedObject> objectVersions = new();
        private readonly List<VersionedObject> objectVersionsOrdered = new();

        public VersionTracker(ObjectDto model)
        {
            this.model = model;
        }

        public VersionedObject Add(string versionName, ObjectDto version)
        {
            var v = ConvertDtoToObject(null, versionName, model, version);
            objectVersions[versionName] = v;
            objectVersionsOrdered.Add(v);
            return v;
        }

        public VersionedObject Update(string versionName, ObjectDto version)
        {
            var existing = objectVersions[versionName];
            UpdateObjectValues(model, existing, version);
            return existing;
        }

        private static void UpdateObjectValues(ObjectDto objectDto, VersionedObject toUpdate, ObjectDto updatedValues)
        {
            var childObjects = updatedValues.Objects.ToDictionary(o => o.Name);
            var objectsToUpdate = objectDto.Objects.Where(o => childObjects.ContainsKey(o.Name));
            foreach (var o in objectsToUpdate)
            {
                UpdateObjectValues(o, toUpdate.GetObject(o.Name), childObjects[o.Name]);
            }

            var childProperties = updatedValues.Properties.ToDictionary(o => o.Name);
            var propertiesToUpdate = objectDto.Properties.Where(p => childProperties.ContainsKey(p.Name));
            foreach (var p in propertiesToUpdate)
            {
                toUpdate.GetProperty(p.Name).SetValue(p.Value);
            }
        }

        private VersionedObject ConvertDtoToObject(
            VersionedObject parentObject,
            string versionName,
            ObjectDto objectModel,
            ObjectDto toConvert)
        {
            if (toConvert == null)
            {
                // object doesn't exist in the input doc.
                return new VersionedObject(objectModel.Name, versionName, EmptyProperties, EmptyObjects, parentObject);
            }

            // hack
            objectModel.Objects ??= new List<ObjectDto>();
            objectModel.Properties ??= new List<PropertyDto>();
            toConvert.Objects ??= new List<ObjectDto>();
            toConvert.Properties ??= new List<PropertyDto>();
            var childObjects = toConvert.Objects.ToDictionary(o => o.Name);
            var childProperties = toConvert.Properties.ToDictionary(o => o.Name);
            var objs = objectModel
                .Objects
                .Select(cm =>
                {
                    var childToConvert = childObjects
                        .ContainsKey(cm.Name)
                        ? childObjects[cm.Name]
                        : null;
                    return ConvertDtoToObject(parentObject, versionName, childToConvert, cm);
                })
                .ToList();

            var properties = objectModel
                .Properties
                .Select(cp =>
                {
                    var propertyToConvert = childProperties
                        .ContainsKey(cp.Name)
                        ? childProperties[cp.Name]
                        : null;
                    return ConvertDtoToProperty(versionName, parentObject, propertyToConvert, cp);
                });

            var obj = new VersionedObject(toConvert.Name, versionName, properties, objs, parentObject);
            var lastVersion = objectVersionsOrdered.LastOrDefault();
            if (lastVersion != null)
            {
                lastVersion.NextVersion = obj;
                obj.PreviousVersion = lastVersion;
            }

            return obj;
        }

        private VersionedProperty ConvertDtoToProperty(
            string versionName,
            VersionedObject parentObject,
            PropertyDto currentProperty,
            PropertyDto currentDto)
        {
            var p = new VersionedProperty(versionName, currentDto.Name, currentProperty?.Value, parentObject);
            var lastVersion = objectVersionsOrdered.LastOrDefault();
            if (lastVersion != null)
            {
                var lastProperty = lastVersion.GetProperty(p.Name);
                lastProperty.NextVersion = p;
                p.PreviousVersion = lastProperty;
            }

            return p;
        }
    }
}