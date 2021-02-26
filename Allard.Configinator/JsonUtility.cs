using System.IO;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator
{
    public static class JsonUtility
    {
        public static JObject GetFile(string fileName)
        {
            fileName.EnsureValue(nameof(fileName));
            return JObject.Parse(File.ReadAllText(fileName));
        }
    }
}