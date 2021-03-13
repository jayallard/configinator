using System;
using System.IO;
using System.Linq;
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
        
        /// <summary>
        /// System.Text.JSon doesn't have a DeepEquals method like
        /// newtonsoft. I'm too lazy to write it.
        /// This is a hack. It converts the json document to a string
        /// of characters in alphabetical order.
        /// This is obviously weak, but works for immediate purposes.
        /// If the same number of characters are available in 2 docs
        /// that have nothing to do with each other, the string will be
        /// the same. But its only used by tests for which this is understoo
        /// and is acceptable.
        ///
        ///     IE:    { "world": "hello" }   will produce the same result as
        ///            { "hello": "world" }
        ///
        /// But, that's not whats being tested. If you try to make it fail,
        /// you will succeed.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static string ToStupidComparisonString(this JsonDocument document)
        {
            var jsonString = document.ConvertToString();
            return
                new string(jsonString
                    //.Where(c => !char.IsWhiteSpace(c))
                    .OrderBy(c => c)
                    .ToArray());
        }
    }
}