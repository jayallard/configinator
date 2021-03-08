using System;
using System.IO;
using System.Text.Json;

namespace Allard.Configinator.Core.Tests
{
    public static class TestUtility
    {
        public static JsonElement GetJsonFile(string fileName)
        {
            return JsonDocument.Parse(GetFile(fileName)).RootElement;
        }

        public static string GetFile(string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "__TestFiles",
                fileName);
            return File.ReadAllText(path);
        }
    }
}