using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class JsonVersionedObject
    {
        private readonly List<JsonProperty> modelObjects;
        private readonly List<JsonProperty> modelProperties;
        private readonly Dictionary<string, JsonVersionedObject> objects = new();
        private readonly Dictionary<string, JsonVersionedProperty> properties = new();

        private JsonVersionedObject(JsonElement model, string path)
        {
            modelProperties = model.GetProperties().ToList();
            foreach (var property in modelProperties)
                properties[property.Name] = new JsonVersionedProperty(path + "/" + property.Name);

            modelObjects = model.GetObjects().ToList();
            foreach (var o in modelObjects)
            {
                var obj = new JsonVersionedObject(o.Value, path + "/" + o.Name);
                objects[o.Name] = obj;
            }
        }

        public JsonVersionedObject(JsonElement model) : this(model, string.Empty)
        {
        }

        public bool IsChanged =>
            properties.Values.Any(p => p.IsChanged)
            || objects.Values.Any(o => o.IsChanged);

        public JsonVersionedObject GetObject(string versionName)
        {
            return objects[versionName];
        }

        public JsonVersionedProperty GetProperty(string propertyName)
        {
            return properties[propertyName];
        }

        public void VisitObjects(
            Action<JsonVersionedProperty> propertyHandler,
            Action<JsonVersionedObject> objectHandler)
        {
            foreach (var p in properties.Values) propertyHandler(p);

            foreach (var o in objects.Values) o.VisitObjects(propertyHandler, objectHandler);
        }

        private void Visit(
            Action<JsonProperty> propertyHandler,
            Action<JsonProperty> objectHandler)
        {
            foreach (var mp in modelProperties) propertyHandler(mp);

            foreach (var mo in modelObjects) objectHandler(mo);
        }

        public void AddVersion(string versionName, JsonElement versionElement)
        {
            Visit(
                p =>
                {
                    var value = versionElement.GetStringProperty(p.Name);
                    properties[p.Name].AddVersion(versionName, value);
                },
                p =>
                {
                    var value = versionElement.GetObjectProperty(p.Name);
                    objects[p.Name].AddVersion(versionName, value);
                });
        }


        public void UpdateVersion(string versionName, JsonElement versionElement)
        {
            Visit(
                p =>
                {
                    var property = properties[p.Name];
                    var value = versionElement.GetStringProperty(p.Name);
                    property.UpdateVersion(versionName, value);
                },
                p =>
                {
                    var obj = objects[p.Name];
                    var value = versionElement.GetObjectProperty(p.Name);
                    obj.UpdateVersion(versionName, value);
                });
        }
    }
}