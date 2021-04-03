using System;
using System.IO;

namespace Allard.Configinator.Core.Tests
{
    public static class TestUtility
    {
        public static string GetFile(string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "__TestFiles",
                fileName);
            return File.ReadAllText(path);
        }
    }
}