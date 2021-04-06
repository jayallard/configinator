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

        public VersionedObject UpdateVersion(string versionName, ObjectDto values, string path = null)
        {
            var existing = objectVersions[versionName];
            var (m, v) = Goto(model, existing, path);


            // hack - PROPERTIES and OBJECTS are different types of objects.
            // IE:
            //      path =    /a/b/c
            //      value =     "update single value"
            // VALUES is just the string value; no name.
            // create a new ObjectDto with the single updated property: name=c, value=value.
            // OOP fail. consider merging VersionedObject and VersionedProperty
            // into a single class... might be too ugly, but look at it.
            if (!string.IsNullOrWhiteSpace(path))
            {
                var last = path.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Last();
                if (m.PropertyExists(last))
                    values = new ObjectDto()
                        .SetName(m.Name)
                        .Add(ObjectDto.CreateString(last, values.Value));
            }


            UpdateObjectValues(m, v, values);
            return existing;
        }

        private static (ObjectDto, VersionedObject) Goto(ObjectDto m, VersionedObject version, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return (m, version);

            var parts = path.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var currentModel = m;
            var currentVersion = version;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                // todo: will throw an exception if path is invalid.
                currentModel = currentModel.GetObject(parts[i]);
                currentVersion = currentVersion.GetObject(parts[i]);
            }

            return (currentModel, currentVersion);
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
            var childObjects = toConvert.Items.ToDictionary(o => o.Name);
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
            ObjectDto currentProperty,
            ObjectDto currentDto)
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