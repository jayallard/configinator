using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class JsonVersionedObject
    {
        private readonly Dictionary<string, JsonVersionedProperty> properties = new();
        private readonly Dictionary<string, JsonVersionedObject> objects = new();
        private readonly List<JsonProperty> modelProperties;
        private readonly List<JsonProperty> modelObjects;

        private JsonVersionedObject(JsonElement model, string path)
        {
            modelProperties = model.GetProperties2().ToList();
            foreach (var property in modelProperties)
            {
                properties[property.Name] = new JsonVersionedProperty(path + "/" + property.Name, property.Value);
            }

            modelObjects = model.GetObjects2().ToList();
            foreach (var o in modelObjects)
            {
                var obj = new JsonVersionedObject(o.Value, path + "/" + o.Name);
                objects[o.Name] = obj;
            }
        }

        public bool IsChanged =>
            properties.Values.Any(p => p.IsChanged)
            || objects.Values.Any(o => o.IsChanged);

        public JsonVersionedObject(JsonElement model) : this(model, string.Empty)
        {
        }

        public JsonVersionedObject GetObject(string versionName)
        {
            return objects[versionName];
        }

        public JsonVersionedProperty GetProperty(string propertyName)
        {
            return properties[propertyName];
        }

        private void Visit(
            Action<JsonProperty> propertyHandler,
            Action<JsonProperty> objectHandler)
        {
            foreach (var mp in modelProperties)
            {
                propertyHandler(mp);
            }

            foreach (var mo in modelObjects)
            {
                objectHandler(mo);
            }
        }

        public void AddVersion(string versionName, JsonElement versionElement)
        {
            Visit(
                p =>
                {
                    var value = versionElement.GetStringProperty2(p.Name);
                    properties[p.Name].AddVersion(versionName, value);
                },
                p =>
                {
                    var value = versionElement.GetObjectProperty2(p.Name);
                    objects[p.Name].AddVersion(versionName, value);
                });
        }

        public void UpdateVersion(string versionName, JsonElement versionElement)
        {
            Visit(
                p =>
                {
                    var property = properties[p.Name];
                    var value = versionElement.GetStringProperty2(p.Name);
                    property.UpdateVersion(versionName, value);
                },
                p =>
                {
                    var obj = objects[p.Name];
                    var value = versionElement.GetObjectProperty2(p.Name);
                    obj.UpdateVersion(versionName, value);
                });
        }
    }
}