using System;
using System.Collections.Generic;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    public class VersionTracker
    {
        private readonly ObjectDto model;
        private readonly Dictionary<string, VersionedObject> objectVersions = new();
        private readonly List<VersionedObject> objectVersionsOrdered = new();

        public VersionTracker(ObjectDto model)
        {
            this.model = model.EnsureValue(nameof(model));
        }

        public IReadOnlyCollection<VersionedObject> Versions => objectVersionsOrdered.ToList();

        public VersionedObject AddVersion(string versionName, ObjectDto version)
        {
            var previousVersion = objectVersionsOrdered.LastOrDefault();
            var v = ConvertDtoToObject(null, previousVersion, versionName, model, version);
            objectVersions[versionName] = v;
            objectVersionsOrdered.Add(v);
            return v;
        }

        public VersionedObject UpdateVersion(string versionName, ObjectDto version, string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                return SetPath(versionName, version, path);
            }
            
            var existing = objectVersions[versionName];
            UpdateObjectValues(model, existing, version);
            return existing;
        }

        private VersionedObject SetPath(string versionName, ObjectDto version, string path)
        {
            var parts = path.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var currentModel = model;
            var baseValue = objectVersions[versionName];
            var currentValue = baseValue;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                // todo: will throw an exception if path is invalid.
                currentModel = currentModel.GetObject(parts[i]);
                currentValue = currentValue.GetObject(parts[i]);
            }

            // if it's an object, then start iterating here.
            if (model.ObjectExists(parts.Last()))
            {
                UpdateObjectValues(currentModel, currentValue, version);
                return baseValue;
            }
            
            // if it's a property, then set the value.
            currentValue
                .GetProperty(parts.Last())
                .SetValue(version.GetProperty(parts.Last()).Value);
            return baseValue;
        }

        private static void UpdateObjectValues(ObjectDto model, VersionedObject toUpdate, ObjectDto updatedValues)
        {
            var childObjects = updatedValues.Objects.ToDictionary(o => o.Name);
            var objectsToUpdate = model.Objects.Where(o => childObjects.ContainsKey(o.Name));
            foreach (var o in objectsToUpdate) UpdateObjectValues(o, toUpdate.GetObject(o.Name), childObjects[o.Name]);

            var childProperties = updatedValues.Properties.ToDictionary(o => o.Name);
            var propertiesToUpdate = model.Properties.Where(p => childProperties.ContainsKey(p.Name));
            foreach (var p in propertiesToUpdate) toUpdate.GetProperty(p.Name).SetValue(childProperties[p.Name].Value);
        }

        private static VersionedObject ConvertDtoToObject(
            VersionedObject parentObject,
            VersionedObject previousVersion,
            string versionName,
            ObjectDto objectModel,
            ObjectDto toConvert)
        {
            // hack
            toConvert ??= new ObjectDto().SetName(objectModel.Name);
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
                    return ConvertDtoToObject(parentObject, previousVersion?.GetObject(cm.Name), versionName,
                        cm, childToConvert);
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
                    return ConvertDtoToProperty(versionName, parentObject, previousVersion, propertyToConvert, cp);
                });

            var obj = new VersionedObject(toConvert.Name, versionName, properties, objs, parentObject);
            if (previousVersion == null) return obj;
            previousVersion.NextVersion = obj;
            obj.PreviousVersion = previousVersion;
            return obj;
        }

        private static VersionedProperty ConvertDtoToProperty(
            string versionName,
            VersionedObject parentObject,
            VersionedObject previousVersion,
            PropertyDto currentProperty,
            PropertyDto currentDto)
        {
            var p = new VersionedProperty(versionName, currentDto.Name, currentProperty?.Value, parentObject);
            if (previousVersion != null)
            {
                var lastProperty = previousVersion.GetProperty(p.Name);
                lastProperty.NextVersion = p;
                p.PreviousVersion = lastProperty;
            }

            return p;
        }
    }
}