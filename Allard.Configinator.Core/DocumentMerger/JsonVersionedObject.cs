using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allard.Configinator.Core.DocumentMerger
{
    public class JsonVersionedObject
    {
        private readonly string path;
        private readonly JsonElement model;
        private readonly Dictionary<string, JsonVersionedProperty> properties = new();
        private readonly Dictionary<string, JsonVersionedObject> objects = new();
        private readonly List<JsonProperty> modelProperties;
        private readonly List<JsonProperty> modelObjects;

        private JsonVersionedObject(JsonElement model, string path)
        {
            this.model = model;
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

        public void AddVersion(string versionName, JsonElement versionElement)
        {
            // TODO: prevent adding the same version multiple times
            foreach (var mp in modelProperties)
            {
                var property = properties[mp.Name];
                var value = versionElement.GetStringProperty2(mp.Name);
                property.AddVersion(versionName, value);
            }

            foreach (var mo in modelObjects)
            {
                var obj = objects[mo.Name];
                var value = versionElement.GetObjectProperty2(mo.Name);
                obj.AddVersion(versionName, value);
            }
        }

        public JsonVersionedObject GetObject(string versionName)
        {
            return objects[versionName];
        }

        public JsonVersionedProperty GetProperty(string propertyName)
        {
            return properties[propertyName];
        }

        public void UpdateVersion(string versionName, JsonElement versionElement)
        {
            foreach (var mp in modelProperties)
            {
                var property = properties[mp.Name];
                var value = versionElement.GetStringProperty2(mp.Name);
                property.UpdateVersion(versionName, value);
            }

            foreach (var mo in modelObjects)
            {
                var obj = objects[mo.Name];
                var value = versionElement.GetObjectProperty2(mo.Name);
                obj.UpdateVersion(versionName, value);
            }
        }
    }
}