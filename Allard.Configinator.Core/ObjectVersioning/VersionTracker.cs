using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Allard.Configinator.Core.ObjectVersioning
{
    [DebuggerDisplay("Name={Name}")]
    public class VersionTracker
    {
        private readonly Node model;
        private readonly Dictionary<string, VersionedNode> objectVersions = new();
        private readonly List<VersionedNode> objectVersionsOrdered = new();

        public VersionTracker(Node model, string name = null)
        {
            this.model = model.EnsureValue(nameof(model));
            Name = name;
        }

        public string Name { get; }

        public IEnumerable<VersionedNode> Versions => objectVersionsOrdered.ToList();

        public void AddVersion(string versionName, Node version)
        {
            var previousVersion = objectVersionsOrdered.LastOrDefault();
            var v = ConvertDtoToObject(null, previousVersion, versionName, model, version);
            objectVersions[versionName] = v;
            objectVersionsOrdered.Add(v);
        }

        public void UpdateVersion(string versionName, Node values, string path = null)
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
                    values = new Node()
                        .SetName(m.Name)
                        .Add(Node.CreateString(last, values.Value));
            }


            UpdateObjectValues(m, v, values);
        }

        private static (Node, VersionedNode) Goto(Node m, VersionedNode version, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return (m, version);

            var parts = path.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var currentModel = m;
            var currentVersion = version;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                // todo: will throw an exception if path is invalid.
                currentModel = currentModel.GetObject(parts[i]);
                currentVersion = currentVersion.GetNode(parts[i]);
            }

            return (currentModel, currentVersion);
        }

        private static void UpdateObjectValues(Node model, VersionedNode toUpdate, Node updatedValues)
        {
            var childObjects = updatedValues.Objects.ToDictionary(o => o.Name);
            var objectsToUpdate = model.Objects.Where(o => childObjects.ContainsKey(o.Name));
            foreach (var o in objectsToUpdate) UpdateObjectValues(o, toUpdate.GetNode(o.Name), childObjects[o.Name]);

            var childProperties = updatedValues.Properties.ToDictionary(o => o.Name);
            var propertiesToUpdate = model.Properties.Where(p => childProperties.ContainsKey(p.Name));
            foreach (var p in propertiesToUpdate) toUpdate.GetProperty(p.Name).SetValue(childProperties[p.Name].Value);
        }

        private static VersionedNode ConvertDtoToObject(
            VersionedNode parentNode,
            VersionedNode previousVersion,
            string versionName,
            Node objectModel,
            Node toConvert)
        {
            // hack
            toConvert ??= new Node().SetName(objectModel.Name);
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
                    return ConvertDtoToObject(parentNode, previousVersion?.GetNode(cm.Name), versionName,
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
                    return ConvertDtoToProperty(versionName, parentNode, previousVersion, propertyToConvert, cp);
                });

            var obj = new VersionedNode(toConvert.Name, versionName, properties, objs, parentNode);
            if (previousVersion == null) return obj;
            previousVersion.NextVersion = obj;
            obj.PreviousVersion = previousVersion;
            return obj;
        }

        private static VersionedProperty ConvertDtoToProperty(
            string versionName,
            VersionedNode parentNode,
            VersionedNode previousVersion,
            Node currentProperty,
            Node currentDto)
        {
            var p = new VersionedProperty(versionName, currentDto.Name, currentProperty?.Value, parentNode);
            if (previousVersion == null) return p;
            var lastProperty = previousVersion.GetProperty(p.Name);
            lastProperty.NextVersion = p;
            p.PreviousVersion = lastProperty;
            return p;
        }
    }
}